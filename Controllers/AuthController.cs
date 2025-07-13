using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoAPI.DTOs;
using YopoAPI.Services;

namespace YopoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IInvitationService _invitationService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(
            IUserService userService, 
            IJwtTokenService jwtTokenService,
            IInvitationService invitationService,
            IPasswordResetService passwordResetService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _invitationService = invitationService;
            _passwordResetService = passwordResetService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.ValidateUserAsync(loginDto.Email, loginDto.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _jwtTokenService.GenerateToken(user);
            var userDto = await _userService.GetUserByIdAsync(user.Id);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = userDto!
            });
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result)
                return BadRequest(new { message = "Current password is incorrect" });

            return Ok(new { message = "Password changed successfully" });
        }



        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponseDto>> Signup(SignupDto signupDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate email or phone is provided
            if (string.IsNullOrEmpty(signupDto.Email) && string.IsNullOrEmpty(signupDto.PhoneNumber))
                return BadRequest(new { message = "Either email or phone number is required" });

            // Check if user already exists
            if (!string.IsNullOrEmpty(signupDto.Email) && await _userService.UserExistsAsync(signupDto.Email))
                return BadRequest(new { message = "User with this email already exists" });

            // Check invitation if provided
            Models.Invitation? invitation = null;
            if (!string.IsNullOrEmpty(signupDto.InvitationToken) && !string.IsNullOrEmpty(signupDto.Email))
            {
                invitation = await _invitationService.GetValidInvitationAsync(signupDto.Email);
                if (invitation == null)
                    return BadRequest(new { message = "Invalid or expired invitation" });
            }
            else
            {
                // For signup without invitation, create a default invitation (for first user or normal user)
                var isFirstUser = await _userService.IsFirstUserAsync();
                var defaultRoleId = isFirstUser ? 1 : 2; // Super Admin for first user, Normal User for others
                
                invitation = new Models.Invitation
                {
                    Email = signupDto.Email ?? string.Empty,
                    PhoneNumber = signupDto.PhoneNumber,
                    RoleId = defaultRoleId,
                    ExpiresAt = DateTime.UtcNow.AddDays(1),
                    IsUsed = false
                };
            }

            var userDto = await _userService.CreateUserWithInvitationAsync(signupDto, invitation);

            // Mark invitation as used if it was provided
            if (invitation.Id > 0)
                await _invitationService.MarkInvitationAsUsedAsync(invitation.Id);

            // Generate token for the new user
            var user = await _userService.ValidateUserByEmailOrPhoneAsync(signupDto.Email ?? signupDto.PhoneNumber!, signupDto.Password);
            if (user == null)
                return BadRequest(new { message = "Signup failed" });

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = userDto
            });
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user exists
            var userDto = await _userService.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (userDto == null)
                return Ok(new { message = "If the email exists, a reset code has been sent" }); // Don't reveal if email exists

            // Generate reset code
            var code = await _passwordResetService.GenerateResetCodeAsync(forgotPasswordDto.Email);

            // TODO: Send email with the code (implement email service)
            // For now, return the code in development (remove in production)
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                return Ok(new { message = "Reset code generated", code = code });
            }

            return Ok(new { message = "If the email exists, a reset code has been sent" });
        }

        [HttpPost("verify-code")]
        public async Task<ActionResult> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValid = await _passwordResetService.VerifyResetCodeAsync(verifyCodeDto.Email, verifyCodeDto.Code);
            if (!isValid)
                return BadRequest(new { message = "Invalid or expired code" });

            return Ok(new { message = "Code verified successfully" });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValidCode = await _passwordResetService.VerifyResetCodeAsync(resetPasswordDto.Email, resetPasswordDto.Code);
            if (!isValidCode)
                return BadRequest(new { message = "Invalid or expired code" });

            var result = await _userService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.NewPassword);
            if (!result)
                return BadRequest(new { message = "Failed to reset password" });

            // Mark code as used
            await _passwordResetService.MarkCodeAsUsedAsync(resetPasswordDto.Email, resetPasswordDto.Code);

            return Ok(new { message = "Password reset successfully" });
        }

        [HttpPost("external-login")]
        public async Task<ActionResult<AuthResponseDto>> ExternalLogin(ExternalLoginDto externalLoginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Implement OAuth validation for Google, Facebook, Apple
            // For now, this is a placeholder implementation
            return BadRequest(new { message = "External login not implemented yet" });
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> RefreshJwtToken()
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var userDto = await _userService.GetUserByIdAsync(userId);

            if (userDto == null)
                return Unauthorized(new { message = "User not found" });

            var user = await _userService.ValidateUserAsync(userDto.Email, "");
            if (user == null)
                return Unauthorized(new { message = "Invalid user" });

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = userDto
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            // TODO: Implement token blacklisting if needed
            // For stateless JWT, logout is handled client-side by removing the token
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpGet("role-check")]
        [Authorize]
        public async Task<ActionResult> RoleCheck([FromQuery] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest(new { message = "Role name is required" });

            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var hasRole = await _userService.HasRoleAsync(userId, roleName);

            return Ok(new { hasRole = hasRole, roleName = roleName });
        }
    }
}
