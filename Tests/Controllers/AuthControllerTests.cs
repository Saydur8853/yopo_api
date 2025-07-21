using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YopoAPI.Modules.Authentication.Controllers;
using YopoAPI.Modules.Authentication.DTOs;
using YopoAPI.Modules.Authentication.Services;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.UserManagement.Models;
using YopoAPI.Modules.RoleManagement.Models;
using YopoAPI.Modules.RoleManagement.DTOs;

namespace YopoAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IInvitationService> _invitationServiceMock;
        private readonly Mock<IPasswordResetService> _passwordResetServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _invitationServiceMock = new Mock<IInvitationService>();
            _passwordResetServiceMock = new Mock<IPasswordResetService>();

            _authController = new AuthController(
                _userServiceMock.Object,
                _jwtTokenServiceMock.Object,
                _invitationServiceMock.Object,
                _passwordResetServiceMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };

            var role = new Role { Id = 1, Name = "User", Description = "Standard User" };
            var user = new User 
            { 
                Id = 1, 
                Email = loginDto.Email, 
                FirstName = "Test", 
                LastName = "User",
                Role = role,
                RoleId = 1
            };
            
            var userDto = new UserDto 
            { 
                Id = 1, 
                Email = loginDto.Email, 
                FirstName = "Test", 
                LastName = "User",
                Role = new RoleDto { Id = 1, Name = "User", Description = "Standard User" }
            };
            
            var token = "test-token";

            _userServiceMock.Setup(x => x.ValidateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(user);

            _userServiceMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(userDto);

            _jwtTokenServiceMock.Setup(x => x.GenerateToken(user))
                .Returns(token);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);

            Assert.Equal(token, response.Token);
            Assert.Equal(loginDto.Email, response.User.Email);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "invalid@example.com", Password = "wrong-password" };

            _userServiceMock.Setup(x => x.ValidateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ReturnsOkWithResetCode()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "test@example.com" };
            var userDto = new UserDto { Id = 1, Email = forgotPasswordDto.Email };
            var resetCode = "123456";

            _userServiceMock.Setup(x => x.GetUserByEmailAsync(forgotPasswordDto.Email))
                .ReturnsAsync(userDto);

            _passwordResetServiceMock.Setup(x => x.GenerateResetCodeAsync(forgotPasswordDto.Email))
                .ReturnsAsync(resetCode);

            // Set environment variable for development
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            // Act
            var result = await _authController.ForgotPassword(forgotPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_WithInvalidEmail_ReturnsOkWithGenericMessage()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };

            _userServiceMock.Setup(x => x.GetUserByEmailAsync(forgotPasswordDto.Email))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _authController.ForgotPassword(forgotPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task VerifyCode_WithValidCode_ReturnsOk()
        {
            // Arrange
            var verifyCodeDto = new VerifyCodeDto { Email = "test@example.com", Code = "123456" };

            _passwordResetServiceMock.Setup(x => x.VerifyResetCodeAsync(verifyCodeDto.Email, verifyCodeDto.Code))
                .ReturnsAsync(true);

            // Act
            var result = await _authController.VerifyCode(verifyCodeDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task VerifyCode_WithInvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var verifyCodeDto = new VerifyCodeDto { Email = "test@example.com", Code = "invalid" };

            _passwordResetServiceMock.Setup(x => x.VerifyResetCodeAsync(verifyCodeDto.Email, verifyCodeDto.Code))
                .ReturnsAsync(false);

            // Act
            var result = await _authController.VerifyCode(verifyCodeDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_WithValidCodeAndPassword_ReturnsOk()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto 
            { 
                Email = "test@example.com", 
                Code = "123456",
                NewPassword = "newPassword123",
                ConfirmNewPassword = "newPassword123"
            };

            _passwordResetServiceMock.Setup(x => x.VerifyResetCodeAsync(resetPasswordDto.Email, resetPasswordDto.Code))
                .ReturnsAsync(true);

            _userServiceMock.Setup(x => x.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.NewPassword))
                .ReturnsAsync(true);

_passwordResetServiceMock.Setup(x => x.MarkCodeAsUsedAsync(resetPasswordDto.Email, resetPasswordDto.Code))
                .ReturnsAsync(true);

            // Act
            var result = await _authController.ResetPassword(resetPasswordDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_WithInvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto 
            { 
                Email = "test@example.com", 
                Code = "invalid",
                NewPassword = "newPassword123",
                ConfirmNewPassword = "newPassword123"
            };

            _passwordResetServiceMock.Setup(x => x.VerifyResetCodeAsync(resetPasswordDto.Email, resetPasswordDto.Code))
                .ReturnsAsync(false);

            // Act
            var result = await _authController.ResetPassword(resetPasswordDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}

