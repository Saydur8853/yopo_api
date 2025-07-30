using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YopoAPI.Data;
using YopoAPI.Modules.Authentication.Services;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.RoleManagement.Services;
using YopoAPI.Modules.PolicyManagement.Services;
using YopoAPI.Middleware;
using DotNetEnv;

// Load environment variables from .env file if it exists
if (File.Exists(".env"))
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Build database connection string from environment variables
string connectionString;

// Check for production AWS RDS configuration first
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_HOSTNAME")))
{
    // Production environment (AWS RDS)
    connectionString = $"Server={Environment.GetEnvironmentVariable("RDS_HOSTNAME")};"
                    + $"Database={Environment.GetEnvironmentVariable("RDS_DB_NAME")};"
                    + $"User={Environment.GetEnvironmentVariable("RDS_USERNAME")};"
                    + $"Password={Environment.GetEnvironmentVariable("RDS_PASSWORD")};"
                    + $"Port={Environment.GetEnvironmentVariable("RDS_PORT")};"
                    + "SslMode=Required;";
}
// Check for local development environment variables
else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_HOST")))
{
    // Local development environment (from .env file)
    connectionString = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};"
                    + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
                    + $"User={Environment.GetEnvironmentVariable("DB_USERNAME")};"
                    + $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};"
                    + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};";
}
else
{
    // Fallback to appsettings.json (not recommended for production)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("No database connection configuration found. Please set environment variables or configure appsettings.json");
}

// Use MySQL database for both development and production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, 
        ServerVersion.Parse("8.0.33-mysql")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

// Add distributed cache (required for session support)
builder.Services.AddDistributedMemoryCache();

// Add data protection for OAuth state management
builder.Services.AddDataProtection();

// Add session support for OAuth state management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for localhost
    options.Cookie.Name = "__yopo_session";
    options.IOTimeout = TimeSpan.FromMinutes(5); // Increase timeout
    options.Cookie.Path = "/";
    options.Cookie.Domain = null; // Let it default to current domain
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/api/auth/google/callback";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for localhost
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "__yopo_auth";
});
// Removed built-in Google authentication to avoid conflicts with custom implementation
// The custom Google OAuth flow is handled by GoogleAuthController.cs

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "yopo",
        Version = "v1",
        Description = "A comprehensive backend API for yopo system"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Apply pending migrations automatically (only for non-in-memory databases)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Only run migrations if not using in-memory database
        if (!context.Database.IsInMemory())
        {
            context.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully.");
        }
        else
        {
            // For in-memory database, ensure it's created
            context.Database.EnsureCreated();
            Console.WriteLine("In-memory database created successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting up database: {ex.Message}");
        // Log the error but don't stop the application
        // You might want to implement proper logging here
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (including production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "yopo v1");
    c.DocumentTitle = "yopo Documentation";
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

app.UseMiddleware<ExceptionMiddleware>();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable static file serving
app.UseStaticFiles();

// Enable session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Add root route to serve the status page
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapControllers();

app.Run();


