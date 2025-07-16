using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;

namespace YopoAPI.Tests;

public class PolicyControllerTests : TestBase
{
    public PolicyControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetTerms_Should_Return_Terms()
    {
        var response = await Client.GetAsync("api/policy/terms");

        response.EnsureSuccessStatusCode();
        var terms = await DeserializeResponse<PolicyDto>(response);
        terms.Type.Should().Be("terms");
    }

    [Fact]
    public async Task GetPrivacyPolicy_Should_Return_PrivacyPolicy()
    {
        var response = await Client.GetAsync("api/policy/privacy");

        response.EnsureSuccessStatusCode();
        var privacy = await DeserializeResponse<PolicyDto>(response);
        privacy.Type.Should().Be("privacy");
    }

    [Fact]
    public async Task GetAllPolicies_Should_Return_All_Policies_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/policy");

        response.EnsureSuccessStatusCode();
        var policies = await DeserializeResponse<IEnumerable<PolicyDto>>(response);
        policies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_Created_Policy_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPolicyDto = new CreatePolicyDto
        {
            Type = "test",
            Content = "Test content",
            Version = "1.0"
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.EnsureSuccessStatusCode();
        var createdPolicy = await DeserializeResponse<PolicyDto>(response);
        createdPolicy.Type.Should().Be(createPolicyDto.Type);
        createdPolicy.Content.Should().Be(createPolicyDto.Content);
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPolicyDto = new CreatePolicyDto
        {
            Type = "", // Invalid type
            Content = "Invalid content",
            Version = "1.0"
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePolicy_Should_Return_Updated_Policy_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePolicyDto = new UpdatePolicyDto
        {
            Content = "Updated content",
            Version = "1.1"
        };

        var response = await Client.PutAsync("api/policy/1", CreateJsonContent(updatePolicyDto));

        response.EnsureSuccessStatusCode();
        var updatedPolicy = await DeserializeResponse<PolicyDto>(response);
        updatedPolicy.Content.Should().Be(updatePolicyDto.Content);
    }

    [Fact]
    public async Task UpdatePolicy_Should_Return_NotFound_For_NonExistent_Policy()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePolicyDto = new UpdatePolicyDto
        {
            Content = "Updated content",
            Version = "1.1"
        };

        var response = await Client.PutAsync("api/policy/999", CreateJsonContent(updatePolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePolicy_Should_Return_NoContent_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/policy/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePolicy_Should_Return_NotFound_For_NonExistent_Policy()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/policy/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllPolicies_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/policy");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllPolicies_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.GetAsync("api/policy");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPolicyDto = new CreatePolicyDto
        {
            Type = "test",
            Content = "Test content",
            Version = "1.0"
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var createPolicyDto = new CreatePolicyDto
        {
            Type = "test",
            Content = "Test content",
            Version = "1.0"
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePolicy_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePolicyDto = new UpdatePolicyDto
        {
            Content = "Updated content",
            Version = "1.1"
        };

        var response = await Client.PutAsync("api/policy/1", CreateJsonContent(updatePolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePolicy_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var updatePolicyDto = new UpdatePolicyDto
        {
            Content = "Updated content",
            Version = "1.1"
        };

        var response = await Client.PutAsync("api/policy/1", CreateJsonContent(updatePolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePolicy_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePolicyDto = new UpdatePolicyDto
        {
            Content = "", // Invalid - empty content
            Version = "1.1"
        };

        var response = await Client.PutAsync("api/policy/1", CreateJsonContent(updatePolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletePolicy_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/policy/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePolicy_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.DeleteAsync("api/policy/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTerms_Should_Return_NotFound_When_Terms_Not_Exist()
    {
        // This test assumes terms policy doesn't exist in test data
        var response = await Client.GetAsync("api/policy/terms");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPrivacyPolicy_Should_Return_NotFound_When_Privacy_Not_Exist()
    {
        // This test assumes privacy policy doesn't exist in test data
        var response = await Client.GetAsync("api/policy/privacy");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_BadRequest_For_Empty_Version()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPolicyDto = new CreatePolicyDto
        {
            Type = "test",
            Content = "Test content",
            Version = "" // Invalid - empty version
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePolicy_Should_Return_BadRequest_For_Empty_Content()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPolicyDto = new CreatePolicyDto
        {
            Type = "test",
            Content = "", // Invalid - empty content
            Version = "1.0"
        };

        var response = await Client.PostAsync("api/policy", CreateJsonContent(createPolicyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
