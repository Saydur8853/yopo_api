using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YopoAPI.Data;
using YopoAPI.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace YopoAPI.Tests;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly ApplicationDbContext Context;

    public TestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Clear existing data
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();

        // Add test roles
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Super Admin", Description = "Full system access" },
            new Role { Id = 2, Name = "Admin", Description = "Admin access" },
            new Role { Id = 3, Name = "Manager", Description = "Manager access" },
            new Role { Id = 4, Name = "Normal User", Description = "Basic user access" }
        };

        Context.Roles.AddRange(roles);

        // Add test users
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                FirstName = "Super",
                LastName = "Admin",
                Email = "superadmin@test.com",
                PasswordHash = BC.HashPassword("password123"),
                RoleId = 1,
                IsSuperAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                PasswordHash = BC.HashPassword("password123"),
                RoleId = 2,
                IsSuperAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                PasswordHash = BC.HashPassword("password123"),
                RoleId = 4,
                IsSuperAdmin = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        Context.Users.AddRange(users);
        Context.SaveChanges();
    }

    protected string GenerateJwtToken(User user, Role role)
    {
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "your-secret-key-here-must-be-at-least-32-characters"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role.Name),
            new Claim("userId", user.Id.ToString()),
            new Claim("roleId", user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "test-issuer",
            audience: configuration["Jwt:Audience"] ?? "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    protected User GetTestUser(int id = 1) => Context.Users.Include(u => u.Role).First(u => u.Id == id);
    protected Role GetTestRole(int id = 1) => Context.Roles.First(r => r.Id == id);

    public void Dispose()
    {
        Scope?.Dispose();
        Context?.Dispose();
        Client?.Dispose();
    }
}
