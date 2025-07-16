using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;
using YopoAPI.Models;
using BC = BCrypt.Net.BCrypt;

namespace YopoAPI.Tests;

public class InvitationsControllerTests : TestBase
{
    public InvitationsControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task CreateInvitation_Should_Return_Created_Invitation_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "newinvite@test.com",
            RoleId = 4,
            PhoneNumber = "+1234567890"
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.EnsureSuccessStatusCode();
        var invitation = await DeserializeResponse<InvitationDto>(response);
        invitation.Email.Should().Be(createInvitationDto.Email);
        invitation.RoleId.Should().Be(createInvitationDto.RoleId);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_BadRequest_For_Invalid_Data()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "", // Invalid - empty email
            RoleId = 4
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "newinvite@test.com",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var createInvitationDto = new CreateInvitationDto
        {
            Email = "newinvite@test.com",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvitations_Should_Return_All_Invitations_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations");

        response.EnsureSuccessStatusCode();
        var invitations = await DeserializeResponse<IEnumerable<InvitationDto>>(response);
        invitations.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvitations_Should_Return_All_Invitations_For_PropertyAdmin()
    {
        var propertyAdminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(propertyAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations");

        response.EnsureSuccessStatusCode();
        var invitations = await DeserializeResponse<IEnumerable<InvitationDto>>(response);
        invitations.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvitations_Should_Return_Forbidden_For_Manager()
    {
        var managerUser = GetTestUser(3);
        var role = GetTestRole(3);
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetInvitations_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.GetAsync("api/invitations");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvitation_Should_Return_Invitation_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create an invitation
        var createInvitationDto = new CreateInvitationDto
        {
            Email = "getinvite@test.com",
            RoleId = 4
        };

        var createResponse = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));
        createResponse.EnsureSuccessStatusCode();
        var createdInvitation = await DeserializeResponse<InvitationDto>(createResponse);

        // Now get the invitation
        var response = await Client.GetAsync($"api/invitations/{createdInvitation.Id}");

        response.EnsureSuccessStatusCode();
        var invitation = await DeserializeResponse<InvitationDto>(response);
        invitation.Email.Should().Be(createInvitationDto.Email);
    }

    [Fact]
    public async Task GetInvitation_Should_Return_NotFound_For_NonExistent_Invitation()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInvitation_Should_Return_Forbidden_For_Manager()
    {
        var managerUser = GetTestUser(3);
        var role = GetTestRole(3);
        var token = GenerateJwtToken(managerUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CheckInvitation_Should_Return_Invitation_Check_For_Valid_Email()
    {
        var response = await Client.GetAsync("api/invitations/check?email=test@test.com");

        response.EnsureSuccessStatusCode();
        var invitationCheck = await DeserializeResponse<InvitationCheckDto>(response);
        invitationCheck.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckInvitation_Should_Return_BadRequest_For_Empty_Email()
    {
        var response = await Client.GetAsync("api/invitations/check?email=");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckInvitation_Should_Return_BadRequest_For_Missing_Email()
    {
        var response = await Client.GetAsync("api/invitations/check");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteInvitation_Should_Return_NoContent_For_SuperAdmin()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First create an invitation to delete
        var createInvitationDto = new CreateInvitationDto
        {
            Email = "deleteinvite@test.com",
            RoleId = 4
        };

        var createResponse = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));
        createResponse.EnsureSuccessStatusCode();
        var createdInvitation = await DeserializeResponse<InvitationDto>(createResponse);

        // Now delete the invitation
        var response = await Client.DeleteAsync($"api/invitations/{createdInvitation.Id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvitation_Should_Return_NotFound_For_NonExistent_Invitation()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/invitations/999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteInvitation_Should_Return_Forbidden_For_Admin()
    {
        var adminUser = GetTestUser(2);
        var role = GetTestRole(2);
        var token = GenerateJwtToken(adminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("api/invitations/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteInvitation_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.DeleteAsync("api/invitations/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvitations_Should_Return_All_Invitations_For_SecurityAdmin()
    {
        // First create a security admin role in test data
        var securityAdminRole = new Role { Id = 5, Name = "Security Admin", Description = "Security Admin access" };
        Context.Roles.Add(securityAdminRole);
        
        var securityAdminUser = new User
        {
            Id = 5,
            FirstName = "Security",
            LastName = "Admin",
            Email = "securityadmin@test.com",
            PasswordHash = BC.HashPassword("password123"),
            RoleId = 5,
            IsSuperAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Users.Add(securityAdminUser);
        Context.SaveChanges();

        var token = GenerateJwtToken(securityAdminUser, securityAdminRole);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("api/invitations");

        response.EnsureSuccessStatusCode();
        var invitations = await DeserializeResponse<IEnumerable<InvitationDto>>(response);
        invitations.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInvitation_Should_Return_Invitation_For_SecurityAdmin()
    {
        // First create a security admin role in test data if not exists
        var existingRole = Context.Roles.FirstOrDefault(r => r.Name == "Security Admin");
        if (existingRole == null)
        {
            var securityAdminRole = new Role { Id = 6, Name = "Security Admin", Description = "Security Admin access" };
            Context.Roles.Add(securityAdminRole);
            Context.SaveChanges();
            existingRole = securityAdminRole;
        }
        
        var securityAdminUser = new User
        {
            Id = 6,
            FirstName = "Security",
            LastName = "Admin",
            Email = "securityadmin2@test.com",
            PasswordHash = BC.HashPassword("password123"),
            RoleId = existingRole.Id,
            IsSuperAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Users.Add(securityAdminUser);
        Context.SaveChanges();

        // First create an invitation as Super Admin
        var superAdminUser = GetTestUser(1);
        var superAdminRole = GetTestRole(1);
        var superAdminToken = GenerateJwtToken(superAdminUser, superAdminRole);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", superAdminToken);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "getinvitesecurityadmin@test.com",
            RoleId = 4
        };

        var createResponse = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));
        createResponse.EnsureSuccessStatusCode();
        var createdInvitation = await DeserializeResponse<InvitationDto>(createResponse);

        // Now get the invitation as Security Admin
        var securityAdminToken = GenerateJwtToken(securityAdminUser, existingRole);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", securityAdminToken);

        var response = await Client.GetAsync($"api/invitations/{createdInvitation.Id}");

        response.EnsureSuccessStatusCode();
        var invitation = await DeserializeResponse<InvitationDto>(response);
        invitation.Email.Should().Be(createInvitationDto.Email);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_BadRequest_For_Invalid_RoleId()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "newinvite@test.com",
            RoleId = 999 // Invalid role ID
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateInvitation_Should_Return_BadRequest_For_Invalid_Email_Format()
    {
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "invalid-email-format",
            RoleId = 4
        };

        var response = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckInvitation_Should_Return_Invitation_Check_For_Nonexistent_Email()
    {
        var response = await Client.GetAsync("api/invitations/check?email=nonexistent@test.com");

        response.EnsureSuccessStatusCode();
        var invitationCheck = await DeserializeResponse<InvitationCheckDto>(response);
        invitationCheck.Should().NotBeNull();
        invitationCheck.HasInvitation.Should().BeFalse();
    }
}
