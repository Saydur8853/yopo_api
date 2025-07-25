using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YopoAPI.Data;
using YopoAPI.Modules.Authentication.Services;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.RoleManagement.Services;
using YopoAPI.Modules.PolicyManagement.Services;
using YopoAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Handle Elastic Beanstalk environment variables for database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Replace environment variable placeholders if they exist
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_HOSTNAME")))
{
    connectionString = $"Server={Environment.GetEnvironmentVariable("RDS_HOSTNAME")};"
                    + $"Database={Environment.GetEnvironmentVariable("RDS_DB_NAME")};"
                    + $"User={Environment.GetEnvironmentVariable("RDS_USERNAME")};"
                    + $"Password={Environment.GetEnvironmentVariable("RDS_PASSWORD")};"
                    + $"Port={Environment.GetEnvironmentVariable("RDS_PORT")};"
                    + "SslMode=Required;";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, 
        ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "YOPO API",
        Version = "v1",
        Description = "A comprehensive backend API for YOPO system"
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

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (including production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "YOPO API v1");
    c.DocumentTitle = "YOPO API Documentation";
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

// Enable static file serving
app.UseStaticFiles();

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


