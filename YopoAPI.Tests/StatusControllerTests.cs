using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.Controllers;

namespace YopoAPI.Tests;

public class StatusControllerTests : TestBase
{
    public StatusControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetApiStatus_Should_Return_Healthy_Status()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_Database_Connected()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.DatabaseStatus.Should().Be("Connected");
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_System_Metrics()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.MemoryUsage.Should().NotBeNullOrWhiteSpace();
        status.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task GetHealthCheck_Should_Return_Healthy()
    {
        var response = await Client.GetAsync("api/status/health");

        response.EnsureSuccessStatusCode();
        var health = await DeserializeResponse<HealthCheckResponse>(response);
        health.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task GetHealthCheck_Should_Contain_Checks()
    {
        var response = await Client.GetAsync("api/status/health");

        response.EnsureSuccessStatusCode();
        var health = await DeserializeResponse<HealthCheckResponse>(response);
        health.Checks.Should().ContainKey("Database");
        health.Checks.Should().ContainKey("API");
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_Response_Time()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.ResponseTime.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_Environment_Information()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.Environment.Should().NotBeNullOrWhiteSpace();
        status.Version.Should().NotBeNullOrWhiteSpace();
        status.ApiName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_Valid_Timestamp()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetHealthCheck_Should_Return_Valid_Timestamp()
    {
        var response = await Client.GetAsync("api/status/health");

        response.EnsureSuccessStatusCode();
        var health = await DeserializeResponse<HealthCheckResponse>(response);
        health.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetApiStatus_Should_Return_All_Required_Fields()
    {
        var response = await Client.GetAsync("api/status");

        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        
        status.Status.Should().NotBeNullOrWhiteSpace();
        status.DatabaseStatus.Should().NotBeNullOrWhiteSpace();
        status.Environment.Should().NotBeNullOrWhiteSpace();
        status.Version.Should().NotBeNullOrWhiteSpace();
        status.ApiName.Should().NotBeNullOrWhiteSpace();
        status.ResponseTime.Should().BeGreaterThan(0);
        status.Timestamp.Should().NotBe(default(DateTime));
    }

    [Fact]
    public async Task GetHealthCheck_Should_Return_All_Required_Fields()
    {
        var response = await Client.GetAsync("api/status/health");

        response.EnsureSuccessStatusCode();
        var health = await DeserializeResponse<HealthCheckResponse>(response);
        
        health.Status.Should().NotBeNullOrWhiteSpace();
        health.Timestamp.Should().NotBe(default(DateTime));
        health.Checks.Should().NotBeNull();
        health.Checks.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetApiStatus_Should_Be_Accessible_Without_Authentication()
    {
        // Status endpoint should be accessible without authentication
        var response = await Client.GetAsync("api/status");
        
        response.EnsureSuccessStatusCode();
        var status = await DeserializeResponse<ApiStatusResponse>(response);
        status.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealthCheck_Should_Be_Accessible_Without_Authentication()
    {
        // Health check endpoint should be accessible without authentication
        var response = await Client.GetAsync("api/status/health");
        
        response.EnsureSuccessStatusCode();
        var health = await DeserializeResponse<HealthCheckResponse>(response);
        health.Should().NotBeNull();
    }
}
