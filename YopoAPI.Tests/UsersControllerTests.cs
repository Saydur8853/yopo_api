using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;

namespace YopoAPI.Tests;

public class UsersControllerTests : TestBase
{
    public UsersControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetUsers_Should_Return_All_Users_For_Admin()
    {
        var adminUser = GetTestUser(2); // Admin user
        var role = GetTestRole(2); // Admin role
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/users");

        response.EnsureSuccessStatusCode();
        var users = await DeserializeResponse<IEnumerable<UserListDto>>(response);
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUsers_Should_Return_Unauthorized_For_Normal_User()
    {
        var normalUser = GetTestUser(3);
        var role = GetTestRole(4);
        var token = GenerateJwtToken(normalUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/users");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_Should_Return_User_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"api/users/{adminUser.Id}");

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(adminUser.Email);
    }

    [Fact]
    public async Task GetUser_Should_Return_NotFound_For_NonExistent_User()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/users/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_Should_Return_Created_User_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            Password = "newpassword123",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/users", CreateJsonContent(createUserDto));

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(createUserDto.Email);
    }

    [Fact]
    public async Task CreateUser_Should_Return_BadRequest_For_Existing_Email()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            FirstName = "New",
            LastName = "User",
            Email = "superadmin@test.com", // Already exists
            Password = "newpassword123",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/users", CreateJsonContent(createUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_Should_Return_Forbidden_For_Manager()
    {
        var managerUser = GetTestUser(2);
        var role = GetTestRole(3); // Manager role
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            Password = "newpassword123",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/users", CreateJsonContent(createUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_Should_Return_Updated_User_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateUserDto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "johnupdated@test.com",
            RoleId = 2,
            IsActive = true
        };

        var response = await Client.PutAsync($"api/users/{adminUser.Id}", CreateJsonContent(updateUserDto));

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(updateUserDto.Email);
    }

    [Fact]
    public async Task UpdateUser_Should_Return_NotFound_For_NonExistent_User()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateUserDto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@test.com",
            RoleId = 2,
            IsActive = true
        };

        var response = await Client.PutAsync("api/users/999", CreateJsonContent(updateUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_Should_Return_NoContent_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync($"api/users/{adminUser.Id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_Should_Return_NotFound_For_NonExistent_User()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/users/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByEmail_Should_Return_User_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"api/users/by-email/{adminUser.Email}");

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(adminUser.Email);
    }

    [Fact]
    public async Task GetUserByEmail_Should_Return_NotFound_For_NonExistent_Email()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/users/by-email/nonexistent@test.com");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleUserStatus_Should_Return_Success_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync($"api/users/{adminUser.Id}/toggle-status", new StringContent(""));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AssignRole_Should_Return_Success_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignRoleDto = new AssignRoleDto
        {
            RoleId = 3
        };

        var response = await Client.PostAsync($"api/users/{superAdminUser.Id}/assign-role", CreateJsonContent(assignRoleDto));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AssignRole_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignRoleDto = new AssignRoleDto
        {
            RoleId = 3
        };

        var response = await Client.PostAsync($"api/users/{adminUser.Id}/assign-role", CreateJsonContent(assignRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_Success_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateStatusDto = new UpdateUserStatusDto
        {
            IsActive = false
        };

        var response = await Client.PostAsync($"api/users/{superAdminUser.Id}/update-status", CreateJsonContent(updateStatusDto));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RemoveRole_Should_Return_Success_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync($"api/users/{superAdminUser.Id}/remove-role");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RemoveRole_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync($"api/users/{adminUser.Id}/remove-role");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_Should_Return_All_Users_For_Manager()
    {
        var managerUser = GetTestUser(3); // Manager user
        var role = GetTestRole(3); // Manager role
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/users");

        response.EnsureSuccessStatusCode();
        var users = await DeserializeResponse<IEnumerable<UserListDto>>(response);
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUser_Should_Return_User_For_Manager()
    {
        var managerUser = GetTestUser(3);
        var role = GetTestRole(3);
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"api/users/{managerUser.Id}");

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(managerUser.Email);
    }

    [Fact]
    public async Task GetUserByEmail_Should_Return_User_For_Manager()
    {
        var managerUser = GetTestUser(3);
        var role = GetTestRole(3);
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"api/users/by-email/{managerUser.Email}");

        response.EnsureSuccessStatusCode();
        var user = await DeserializeResponse<UserDto>(response);
        user.Email.Should().Be(managerUser.Email);
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_For_Duplicate_Email()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateUserDto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "superadmin@test.com", // This email already exists
            RoleId = 2,
            IsActive = true
        };

        var response = await Client.PutAsync($"api/users/{adminUser.Id}", CreateJsonContent(updateUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_Should_Return_BadRequest_For_Invalid_Data()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateUserDto = new UpdateUserDto
        {
            FirstName = "", // Invalid - empty first name
            LastName = "User",
            Email = "valid@test.com",
            RoleId = 2,
            IsActive = true
        };

        var response = await Client.PutAsync($"api/users/{adminUser.Id}", CreateJsonContent(updateUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_Should_Return_BadRequest_For_Invalid_Data()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            FirstName = "", // Invalid - empty first name
            LastName = "User",
            Email = "newuser@test.com",
            Password = "newpassword123",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/users", CreateJsonContent(createUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_Should_Return_BadRequest_For_Invalid_RoleId()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            Password = "newpassword123",
            RoleId = 999 // Invalid role ID
        };

        var response = await Client.PostAsync("api/users", CreateJsonContent(createUserDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ToggleUserStatus_Should_Return_NotFound_For_NonExistent_User()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync("api/users/999/toggle-status", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRole_Should_Return_NotFound_For_NonExistent_User()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignRoleDto = new AssignRoleDto
        {
            RoleId = 3
        };

        var response = await Client.PostAsync("api/users/999/assign-role", CreateJsonContent(assignRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRole_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var assignRoleDto = new AssignRoleDto
        {
            RoleId = 999 // Invalid role ID
        };

        var response = await Client.PostAsync($"api/users/{superAdminUser.Id}/assign-role", CreateJsonContent(assignRoleDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_NotFound_For_NonExistent_User()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateStatusDto = new UpdateUserStatusDto
        {
            IsActive = false
        };

        var response = await Client.PostAsync("api/users/999/update-status", CreateJsonContent(updateStatusDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Send invalid JSON or missing data
        var response = await Client.PostAsync($"api/users/{superAdminUser.Id}/update-status", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveRole_Should_Return_NotFound_For_NonExistent_User()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/users/999/remove-role");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task All_Endpoints_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Test multiple endpoints without authentication
        var endpoints = new[]
        {
            "api/users",
            "api/users/1",
            "api/users/by-email/test@test.com"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task ToggleUserStatus_Should_Return_Forbidden_For_Manager()
    {
        var managerUser = GetTestUser(3);
        var role = GetTestRole(3);
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync($"api/users/{managerUser.Id}/toggle-status", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateStatusDto = new UpdateUserStatusDto
        {
            IsActive = false
        };

        var response = await Client.PostAsync($"api/users/{adminUser.Id}/update-status", CreateJsonContent(updateStatusDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }
}

