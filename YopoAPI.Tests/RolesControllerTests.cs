using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;

namespace YopoAPI.Tests;

public class RolesControllerTests : TestBase
{
    public RolesControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetRoles_Should_Return_All_Roles_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles");

        response.EnsureSuccessStatusCode();
        var roles = await DeserializeResponse<IEnumerable<RoleDto>>(response);
        roles.Should().NotBeEmpty();
        roles.Count().Should().Be(4); // We have 4 roles in test data
    }

    [Fact]
    public async Task GetRoles_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRole_Should_Return_Role_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles/1");

        response.EnsureSuccessStatusCode();
        var roleDto = await DeserializeResponse<RoleDto>(response);
        roleDto.Id.Should().Be(1);
        roleDto.Name.Should().Be("Super Admin");
    }

    [Fact]
    public async Task GetRole_Should_Return_NotFound_For_NonExistent_Role()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateRole_Should_Return_Created_Role_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRoleDto = new CreateRoleDto
        {
            Name = "New Role",
            Description = "New role description"
        };

        var response = await Client.PostAsync("api/roles", CreateJsonContent(createRoleDto));

        response.EnsureSuccessStatusCode();
        var createdRole = await DeserializeResponse<RoleDto>(response);
        createdRole.Name.Should().Be(createRoleDto.Name);
        createdRole.Description.Should().Be(createRoleDto.Description);
    }

    [Fact]
    public async Task CreateRole_Should_Return_BadRequest_For_Duplicate_Name()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRoleDto = new CreateRoleDto
        {
            Name = "Admin", // Already exists
            Description = "Duplicate role"
        };

        var response = await Client.PostAsync("api/roles", CreateJsonContent(createRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRole_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRoleDto = new CreateRoleDto
        {
            Name = "", // Invalid - empty name
            Description = "Invalid role"
        };

        var response = await Client.PostAsync("api/roles", CreateJsonContent(createRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateRole_Should_Return_Updated_Role_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateRoleDto = new UpdateRoleDto
        {
            Name = "Updated Role",
            Description = "Updated description"
        };

        var response = await Client.PutAsync("api/roles/4", CreateJsonContent(updateRoleDto));

        response.EnsureSuccessStatusCode();
        var updatedRole = await DeserializeResponse<RoleDto>(response);
        updatedRole.Name.Should().Be(updateRoleDto.Name);
        updatedRole.Description.Should().Be(updateRoleDto.Description);
    }

    [Fact]
    public async Task UpdateRole_Should_Return_NotFound_For_NonExistent_Role()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateRoleDto = new UpdateRoleDto
        {
            Name = "Updated Role",
            Description = "Updated description"
        };

        var response = await Client.PutAsync("api/roles/999", CreateJsonContent(updateRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_Should_Return_BadRequest_When_Role_Has_Users()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Try to delete Super Admin role which has users
        var response = await Client.DeleteAsync("api/roles/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteRole_Should_Return_BadRequest_For_NonExistent_Role()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/roles/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignPrivilegesToRole_Should_Return_Success_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignPrivilegesDto = new AssignPrivilegesToRoleDto
        {
            PrivilegeIds = new List<int> { 1, 2, 3 }
        };

        var response = await Client.PostAsync("api/roles/4/assign-privileges", CreateJsonContent(assignPrivilegesDto));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AssignPrivilegesToRole_Should_Return_NotFound_For_NonExistent_Role()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignPrivilegesDto = new AssignPrivilegesToRoleDto
        {
            PrivilegeIds = new List<int> { 1, 2, 3 }
        };

        var response = await Client.PostAsync("api/roles/999/assign-privileges", CreateJsonContent(assignPrivilegesDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRolePrivileges_Should_Return_Privileges_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles/1/privileges");

        response.EnsureSuccessStatusCode();
        var privileges = await DeserializeResponse<IEnumerable<PrivilegeDto>>(response);
        // The response may be empty if no privileges are assigned to the role
        privileges.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRolePrivileges_Should_Return_NotFound_For_NonExistent_Role()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/roles/999/privileges");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetRoleHierarchy_Should_Return_Success_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var roleHierarchyDto = new RoleHierarchyDto
        {
            RoleId = 4,
            ParentRoleId = 3,
            HierarchyLevel = 2
        };

        var response = await Client.PostAsync("api/roles/hierarchy", CreateJsonContent(roleHierarchyDto));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SetRoleHierarchy_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var roleHierarchyDto = new RoleHierarchyDto
        {
            RoleId = 999, // Non-existent role
            ParentRoleId = 3,
            HierarchyLevel = 2
        };

        var response = await Client.PostAsync("api/roles/hierarchy", CreateJsonContent(roleHierarchyDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task All_Endpoints_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Test all endpoints without authentication
        var endpoints = new[]
        {
            "api/roles",
            "api/roles/1",
            "api/roles/1/privileges"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}
