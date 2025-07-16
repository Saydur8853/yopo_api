using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using FluentAssertions;
using YopoAPI.DTOs;

namespace YopoAPI.Tests;

public class AuthControllerTests : TestBase
{
    public AuthControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
    {
        var loginDto = new LoginDto
        {
            Email = "superadmin@test.com",
            Password = "password123"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.EnsureSuccessStatusCode();

        var result = await DeserializeResponse<AuthResponseDto>(response);
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be(loginDto.Email);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_When_Credentials_Are_Invalid()
    {
        var loginDto = new LoginDto
        {
            Email = "superadmin@test.com",
            Password = "wrongpassword"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Email_Is_Missing()
    {
        var loginDto = new LoginDto
        {
            Email = "",
            Password = "password123"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Password_Is_Missing()
    {
        var loginDto = new LoginDto
        {
            Email = "superadmin@test.com",
            Password = ""
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_When_User_Is_Inactive()
    {
        var loginDto = new LoginDto
        {
            Email = "jane@test.com", // This user is inactive in test data
            Password = "password123"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Signup_Should_Create_User_And_Return_Token()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.EnsureSuccessStatusCode();

        var result = await DeserializeResponse<AuthResponseDto>(response);
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be(signupDto.Email);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "superadmin@test.com", // Already exists
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_When_Neither_Email_Nor_Phone_Provided()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_Success_When_Current_Password_Is_Correct()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "password123",
            NewPassword = "newpassword123",
            ConfirmNewPassword = "newpassword123"
        };

        // Act
        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ChangePassword_Should_Return_BadRequest_When_Current_Password_Is_Incorrect()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword123",
            ConfirmNewPassword = "newpassword123"
        };

        // Act
        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_Unauthorized_When_No_Token_Provided()
    {
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "password123",
            NewPassword = "newpassword123",
            ConfirmNewPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_Success_When_Email_Is_Valid()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "superadmin@test.com"
        };

        var response = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_Success_Even_When_Email_Does_Not_Exist()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "nonexistent@test.com"
        };

        var response = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));

        response.EnsureSuccessStatusCode(); // Should return success to not reveal if email exists
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_User_When_Authenticated()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("api/auth/me");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<UserDto>(response);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.GetAsync("api/auth/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RoleCheck_Should_Return_True_When_User_Has_Role()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("api/auth/role-check?roleName=Super Admin");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RoleCheck_Should_Return_BadRequest_When_RoleName_Is_Empty()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("api/auth/role-check?roleName=");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_New_Token_When_Authenticated()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.PostAsync("api/auth/refresh-token", new StringContent(""));

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<AuthResponseDto>(response);
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Logout_Should_Return_Success_When_Authenticated()
    {
        // Arrange
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.PostAsync("api/auth/logout", new StringContent(""));

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Logout_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.PostAsync("api/auth/logout", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyCode_Should_Return_Success_For_Valid_Code()
    {
        // First generate a reset code
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "superadmin@test.com"
        };

        var forgotResponse = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));
        forgotResponse.EnsureSuccessStatusCode();

        // In development, the code is returned in the response
        var forgotResult = await DeserializeResponse<dynamic>(forgotResponse);
        var code = forgotResult.code?.ToString();

        if (!string.IsNullOrEmpty(code))
        {
            var verifyCodeDto = new VerifyCodeDto
            {
                Email = "superadmin@test.com",
                Code = code
            };

            var response = await Client.PostAsync("api/auth/verify-code", CreateJsonContent(verifyCodeDto));

            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task VerifyCode_Should_Return_BadRequest_For_Invalid_Code()
    {
        var verifyCodeDto = new VerifyCodeDto
        {
            Email = "superadmin@test.com",
            Code = "invalid_code"
        };

        var response = await Client.PostAsync("api/auth/verify-code", CreateJsonContent(verifyCodeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyCode_Should_Return_BadRequest_For_Empty_Email()
    {
        var verifyCodeDto = new VerifyCodeDto
        {
            Email = "",
            Code = "123456"
        };

        var response = await Client.PostAsync("api/auth/verify-code", CreateJsonContent(verifyCodeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyCode_Should_Return_BadRequest_For_Empty_Code()
    {
        var verifyCodeDto = new VerifyCodeDto
        {
            Email = "superadmin@test.com",
            Code = ""
        };

        var response = await Client.PostAsync("api/auth/verify-code", CreateJsonContent(verifyCodeDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_Success_For_Valid_Code()
    {
        // First generate a reset code
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "superadmin@test.com"
        };

        var forgotResponse = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));
        forgotResponse.EnsureSuccessStatusCode();

        // In development, the code is returned in the response
        var forgotResult = await DeserializeResponse<dynamic>(forgotResponse);
        var code = forgotResult.code?.ToString();

        if (!string.IsNullOrEmpty(code))
        {
            var resetPasswordDto = new ResetPasswordDto
            {
                Email = "superadmin@test.com",
                Code = code,
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };

            var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_For_Invalid_Code()
    {
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "superadmin@test.com",
            Code = "invalid_code",
            NewPassword = "newpassword123",
            ConfirmPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_For_Empty_Email()
    {
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "",
            Code = "123456",
            NewPassword = "newpassword123",
            ConfirmPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_For_Empty_Code()
    {
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "superadmin@test.com",
            Code = "",
            NewPassword = "newpassword123",
            ConfirmPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_For_Empty_NewPassword()
    {
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "superadmin@test.com",
            Code = "123456",
            NewPassword = "",
            ConfirmPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExternalLogin_Should_Return_BadRequest_Not_Implemented()
    {
        var externalLoginDto = new ExternalLoginDto
        {
            Provider = "Google",
            Token = "test_token",
            Email = "test@test.com"
        };

        var response = await Client.PostAsync("api/auth/external-login", CreateJsonContent(externalLoginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExternalLogin_Should_Return_BadRequest_For_Invalid_Data()
    {
        var externalLoginDto = new ExternalLoginDto
        {
            Provider = "", // Invalid - empty provider
            Token = "test_token",
            Email = "test@test.com"
        };

        var response = await Client.PostAsync("api/auth/external-login", CreateJsonContent(externalLoginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.PostAsync("api/auth/refresh-token", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RoleCheck_Should_Return_False_When_User_Does_Not_Have_Role()
    {
        // Arrange
        var user = GetTestUser(3); // Normal user
        var role = GetTestRole(4); // Normal user role
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("api/auth/role-check?roleName=Super Admin");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<dynamic>(response);
        result.hasRole.Should().Be(false);
    }

    [Fact]
    public async Task RoleCheck_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var response = await Client.GetAsync("api/auth/role-check?roleName=Super Admin");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Invalid_Data()
    {
        var signupDto = new SignupDto
        {
            FirstName = "", // Invalid - empty first name
            LastName = "User",
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Password_Mismatch()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "differentpassword" // Password mismatch
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Create_User_With_PhoneNumber_Only()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<AuthResponseDto>(response);
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.PhoneNumber.Should().Be(signupDto.PhoneNumber);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_BadRequest_For_Invalid_Email()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "" // Invalid - empty email
        };

        var response = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Create_User_With_Valid_Invitation_Token()
    {
        // First create an invitation
        var superAdminUser = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(superAdminUser, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createInvitationDto = new CreateInvitationDto
        {
            Email = "inviteduser@test.com",
            RoleId = 4
        };

        var invitationResponse = await Client.PostAsync("api/invitations", CreateJsonContent(createInvitationDto));
        invitationResponse.EnsureSuccessStatusCode();
        var invitation = await DeserializeResponse<InvitationDto>(invitationResponse);

        // Clear authentication for signup
        Client.DefaultRequestHeaders.Authorization = null;

        // Now signup with invitation token
        var signupDto = new SignupDto
        {
            FirstName = "Invited",
            LastName = "User",
            Email = "inviteduser@test.com",
            Password = "password123",
            ConfirmPassword = "password123",
            InvitationToken = invitation.Token
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.EnsureSuccessStatusCode();
        var result = await DeserializeResponse<AuthResponseDto>(response);
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be(signupDto.Email);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Invalid_Invitation_Token()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "password123",
            InvitationToken = "invalid-token"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_For_Invalid_Email_Format()
    {
        var loginDto = new LoginDto
        {
            Email = "invalid-email-format",
            Password = "password123"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_BadRequest_For_Password_Mismatch()
    {
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "password123",
            NewPassword = "newpassword123",
            ConfirmNewPassword = "differentpassword" // Mismatch
        };

        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_For_Password_Mismatch()
    {
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "superadmin@test.com",
            Code = "123456",
            NewPassword = "newpassword123",
            ConfirmPassword = "differentpassword" // Mismatch
        };

        var response = await Client.PostAsync("api/auth/reset-password", CreateJsonContent(resetPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_BadRequest_For_Invalid_Email_Format()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "invalid-email-format"
        };

        var response = await Client.PostAsync("api/auth/forgot-password", CreateJsonContent(forgotPasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_BadRequest_For_Empty_CurrentPassword()
    {
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "", // Empty current password
            NewPassword = "newpassword123",
            ConfirmNewPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_BadRequest_For_Empty_NewPassword()
    {
        var user = GetTestUser(1);
        var role = GetTestRole(1);
        var token = GenerateJwtToken(user, role);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "password123",
            NewPassword = "", // Empty new password
            ConfirmNewPassword = "newpassword123"
        };

        var response = await Client.PostAsync("api/auth/change-password", CreateJsonContent(changePasswordDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Empty_FirstName()
    {
        var signupDto = new SignupDto
        {
            FirstName = "", // Empty first name
            LastName = "User",
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Empty_LastName()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "", // Empty last name
            Email = "testuser@test.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Empty_Password()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@test.com",
            Password = "", // Empty password
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Signup_Should_Return_BadRequest_For_Invalid_Email_Format()
    {
        var signupDto = new SignupDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email-format",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        var response = await Client.PostAsync("api/auth/signup", CreateJsonContent(signupDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_Should_Return_Unauthorized_For_Invalid_Token()
    {
        // Set an invalid token
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await Client.PostAsync("api/auth/refresh-token", new StringContent(""));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Unauthorized_For_Invalid_Token()
    {
        // Set an invalid token
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await Client.GetAsync("api/auth/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RoleCheck_Should_Return_Unauthorized_For_Invalid_Token()
    {
        // Set an invalid token
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await Client.GetAsync("api/auth/role-check?roleName=Super Admin");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_For_Empty_Email()
    {
        var loginDto = new LoginDto
        {
            Email = "",
            Password = "password123"
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_For_Empty_Password()
    {
        var loginDto = new LoginDto
        {
            Email = "superadmin@test.com",
            Password = ""
        };

        var response = await Client.PostAsync("api/auth/login", CreateJsonContent(loginDto));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}

