using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Data.Common;
using YopoAPI.Data;

namespace YopoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public StatusController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<ApiStatusResponse>> GetApiStatus()
        {
            var stopwatch = Stopwatch.StartNew();
            var status = new ApiStatusResponse
            {
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            try
            {
                // Check database connectivity
                await _context.Database.OpenConnectionAsync();
                var connection = _context.Database.GetDbConnection();
                status.DatabaseName = connection.Database;
                await _context.Database.CloseConnectionAsync();
                status.DatabaseStatus = "Connected";
            }
            catch (Exception ex)
            {
                status.DatabaseStatus = $"Error: {ex.Message}";
                status.Status = "Degraded";
                // Try to get database name from connection string even if connection fails
                try
                {
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    if (connectionString != null && connectionString.Contains("Database="))
                    {
                        var dbNameStart = connectionString.IndexOf("Database=") + "Database=".Length;
                        var dbNameEnd = connectionString.IndexOf(";", dbNameStart);
                        if (dbNameEnd == -1) dbNameEnd = connectionString.Length;
                        status.DatabaseName = connectionString.Substring(dbNameStart, dbNameEnd - dbNameStart);
                    }
                }
                catch
                {
                    status.DatabaseName = "Unknown";
                }
            }

            // Check system metrics
            try
            {
                var process = Process.GetCurrentProcess();
                status.MemoryUsage = $"{process.WorkingSet64 / 1024 / 1024} MB";
                status.Uptime = DateTime.Now - process.StartTime;
            }
            catch (Exception ex)
            {
                status.SystemMetrics = $"Error getting metrics: {ex.Message}";
            }

            stopwatch.Stop();
            status.ResponseTime = stopwatch.ElapsedMilliseconds;

            // Set overall status
            if (status.Status == "Healthy" && status.DatabaseStatus.StartsWith("Error"))
            {
                status.Status = "Degraded";
            }

            return Ok(status);
        }

        [HttpGet("health")]
        public async Task<ActionResult<HealthCheckResponse>> GetHealthCheck()
        {
            var health = new HealthCheckResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Checks = new Dictionary<string, string>()
            };

            try
            {
                // Database health check
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                health.Checks.Add("Database", "Healthy");
            }
            catch (Exception ex)
            {
                health.Checks.Add("Database", $"Unhealthy: {ex.Message}");
                health.Status = "Unhealthy";
            }

            // API endpoint health check
            try
            {
                // Simple check to ensure the API can respond
                health.Checks.Add("API", "Healthy");
            }
            catch (Exception ex)
            {
                health.Checks.Add("API", $"Unhealthy: {ex.Message}");
                health.Status = "Unhealthy";
            }

            return Ok(health);
        }
    }

    public class ApiStatusResponse
    {
        public string Status { get; set; } = "Healthy";
        public DateTime Timestamp { get; set; }
        public string Environment { get; set; } = "";
        public string DatabaseStatus { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string MemoryUsage { get; set; } = "";
        public TimeSpan Uptime { get; set; }
        public long ResponseTime { get; set; }
        public string SystemMetrics { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string ApiName { get; set; } = "YOPO API";
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Healthy";
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Checks { get; set; } = new();
    }
}

