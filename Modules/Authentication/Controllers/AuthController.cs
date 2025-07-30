using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using YopoAPI.Modules.Authentication.DTOs;
using YopoAPI.Modules.Authentication.Services;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.UserManagement.Models;

namespace YopoAPI.Modules.Authentication.Controllers
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

            // Check if this is the first user
            var isFirstUser = await _userService.IsFirstUserAsync();
            
            Invitation? invitation = null;
            
            if (isFirstUser)
            {
                // First user signup - automatically becomes superuser, no invitation needed
                invitation = new Invitation
                {
                    Email = signupDto.Email ?? string.Empty,
                    PhoneNumber = signupDto.PhoneNumber,
                    RoleId = 1, // Super Admin role
                    ExpiresAt = DateTime.UtcNow.AddDays(1),
                    IsUsed = false
                };
            }
            else
            {
                // For all subsequent users, invitation is required
                if (string.IsNullOrEmpty(signupDto.Email))
                    return BadRequest(new { message = "Email is required for invitation-based signup" });
                    
                invitation = await _invitationService.GetValidInvitationAsync(signupDto.Email);
                if (invitation == null)
                    return BadRequest(new { message = "This email is not invited, please contact Admin" });
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

            // TODO: Implement proper OAuth validation here if needed

            var email = externalLoginDto.Email;
            var firstName = externalLoginDto.FirstName;
            var lastName = externalLoginDto.LastName;
            var picture = externalLoginDto.ProviderId; // Assuming ProviderId holds the picture URL

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required for external login" });
            }

            var existingUser = await _userService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                var user = await _userService.GetActiveUserByEmailAsync(email);
                if (user == null)
                {
                    return Unauthorized(new { message = "User account is inactive" });
                }

                if (!string.IsNullOrEmpty(picture) && user.ProfilePicture != picture)
                {
                    user.ProfilePicture = picture;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userService.UpdateUserAsync(user.Id, new UpdateUserDto
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        IsActive = user.IsActive
                    });
                }

                var token = _jwtTokenService.GenerateToken(user);
                var userDto = await _userService.GetUserByIdAsync(user.Id);

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(24),
                    User = userDto!
                });
            }
            else
            {
                var isFirstUser = await _userService.IsFirstUserAsync();
                Invitation? invitation;

                if (isFirstUser)
                {
                    invitation = new Invitation
                    {
                        Email = email,
                        RoleId = 1, // Super Admin
                        ExpiresAt = DateTime.UtcNow.AddDays(1),
                        IsUsed = false
                    };
                }
                else
                {
                    invitation = await _invitationService.GetValidInvitationAsync(email);
                    if (invitation == null)
                    {
                        return BadRequest(new { message = "This email is not invited, please contact Admin" });
                    }
                }

                var signupDto = new SignupDto
                {
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    Email = email,
                    Password = Guid.NewGuid().ToString(),
                    ConfirmPassword = Guid.NewGuid().ToString()
                };
                var tempPassword = Guid.NewGuid().ToString();
                signupDto.Password = tempPassword;
                signupDto.ConfirmPassword = tempPassword;

                var userDto = await _userService.CreateUserWithInvitationAsync(signupDto, invitation, picture);

                if (invitation.Id > 0)
                {
                    await _invitationService.MarkInvitationAsUsedAsync(invitation.Id);
                }

                var newUser = await _userService.GetActiveUserByEmailAsync(email);
                if (newUser == null)
                {
                    return BadRequest(new { message = "User creation failed" });
                }

                var token = _jwtTokenService.GenerateToken(newUser);
                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(24),
                    User = userDto
                });
            }
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

        [HttpGet("signin-google")]
        public IActionResult SignInGoogle()
        {
            try
            {
                Console.WriteLine("=== Starting Custom Google OAuth Flow ===");
                Console.WriteLine($"Session ID: {HttpContext.Session.Id}");
                Console.WriteLine($"Request URL: {Request.Scheme}://{Request.Host}{Request.Path}");
                
                // Generate a custom state parameter
                var state = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("oauth_state", state);
                
                // Build Google OAuth URL manually
                var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID environment variable is not set");
                var redirectUri = "http://localhost:5260/auth/google/callback";
                var scope = "openid profile email";
                
                var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                    $"?client_id={Uri.EscapeDataString(clientId)}" +
                    $"&response_type=code" +
                    $"&scope={Uri.EscapeDataString(scope)}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                    $"&state={Uri.EscapeDataString(state)}";
                
                Console.WriteLine($"Custom OAuth URL: {googleAuthUrl}");
                Console.WriteLine($"Generated state: {state}");
                
                return Redirect(googleAuthUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating Google OAuth: {ex.Message}");
                return Redirect($"/?error=oauth_initiation_failed&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

    }
}
