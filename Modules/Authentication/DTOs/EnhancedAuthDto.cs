using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Modules.Authentication.DTOs
{
    public class SignupDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(6)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? InvitationToken { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyCodeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [StringLength(100)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ExternalLoginDto
    {
        [Required]
        public string Provider { get; set; } = string.Empty; // "Google", "Facebook", "Apple"

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProviderId { get; set; }
    }

    public class RoleCheckDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        [Required]
        public int RoleId { get; set; }
    }

    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}


