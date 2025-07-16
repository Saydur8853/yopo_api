using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;

namespace YopoAPI.Tests;

public class PrivilegesControllerTests : TestBase
{
    public PrivilegesControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetPrivileges_Should_Return_All_Privileges_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/privileges");

        response.EnsureSuccessStatusCode();
        var privileges = await DeserializeResponse<IEnumerable<PrivilegeDto>>(response);
        privileges.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPrivileges_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/privileges");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPrivileges_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.GetAsync("api/privileges");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPrivilege_Should_Return_Privilege_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a privilege to get
        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "Test Privilege",
            Description = "Test privilege description",
            Category = "Test"
        };

        var createResponse = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));
        createResponse.EnsureSuccessStatusCode();
        var createdPrivilege = await DeserializeResponse<PrivilegeDto>(createResponse);

        // Now get the privilege
        var response = await Client.GetAsync($"api/privileges/{createdPrivilege.Id}");

        response.EnsureSuccessStatusCode();
        var privilege = await DeserializeResponse<PrivilegeDto>(response);
        privilege.Name.Should().Be(createPrivilegeDto.Name);
    }

    [Fact]
    public async Task GetPrivilege_Should_Return_NotFound_For_NonExistent_Privilege()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/privileges/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePrivilege_Should_Return_Created_Privilege_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "New Privilege",
            Description = "New privilege description",
            Category = "Test"
        };

        var response = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));

        response.EnsureSuccessStatusCode();
        var createdPrivilege = await DeserializeResponse<PrivilegeDto>(response);
        createdPrivilege.Name.Should().Be(createPrivilegeDto.Name);
        createdPrivilege.Description.Should().Be(createPrivilegeDto.Description);
        createdPrivilege.Category.Should().Be(createPrivilegeDto.Category);
    }

    [Fact]
    public async Task CreatePrivilege_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "", // Invalid - empty name
            Description = "Invalid privilege",
            Category = "Test"
        };

        var response = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePrivilege_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "New Privilege",
            Description = "New privilege description",
            Category = "Test"
        };

        var response = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePrivilege_Should_Return_Updated_Privilege_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a privilege to update
        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "Original Privilege",
            Description = "Original description",
            Category = "Test"
        };

        var createResponse = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));
        createResponse.EnsureSuccessStatusCode();
        var createdPrivilege = await DeserializeResponse<PrivilegeDto>(createResponse);

        // Now update the privilege
        var updatePrivilegeDto = new UpdatePrivilegeDto
        {
            Name = "Updated Privilege",
            Description = "Updated description",
            Category = "Updated"
        };

        var response = await Client.PutAsync($"api/privileges/{createdPrivilege.Id}", CreateJsonContent(updatePrivilegeDto));

        response.EnsureSuccessStatusCode();
        var updatedPrivilege = await DeserializeResponse<PrivilegeDto>(response);
        updatedPrivilege.Name.Should().Be(updatePrivilegeDto.Name);
        updatedPrivilege.Description.Should().Be(updatePrivilegeDto.Description);
        updatedPrivilege.Category.Should().Be(updatePrivilegeDto.Category);
    }

    [Fact]
    public async Task UpdatePrivilege_Should_Return_NotFound_For_NonExistent_Privilege()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePrivilegeDto = new UpdatePrivilegeDto
        {
            Name = "Updated Privilege",
            Description = "Updated description",
            Category = "Updated"
        };

        var response = await Client.PutAsync("api/privileges/999", CreateJsonContent(updatePrivilegeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePrivilege_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updatePrivilegeDto = new UpdatePrivilegeDto
        {
            Name = "", // Invalid - empty name
            Description = "Invalid privilege",
            Category = "Test"
        };

        var response = await Client.PutAsync("api/privileges/1", CreateJsonContent(updatePrivilegeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletePrivilege_Should_Return_NoContent_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create a privilege to delete
        var createPrivilegeDto = new CreatePrivilegeDto
        {
            Name = "Privilege to Delete",
            Description = "This privilege will be deleted",
            Category = "Test"
        };

        var createResponse = await Client.PostAsync("api/privileges", CreateJsonContent(createPrivilegeDto));
        createResponse.EnsureSuccessStatusCode();
        var createdPrivilege = await DeserializeResponse<PrivilegeDto>(createResponse);

        // Now delete the privilege
        var response = await Client.DeleteAsync($"api/privileges/{createdPrivilege.Id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePrivilege_Should_Return_NotFound_For_NonExistent_Privilege()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/privileges/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePrivilege_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/privileges/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }
}
