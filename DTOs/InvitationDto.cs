using System.ComponentModel.DataAnnotations;

namespace YopoAPI.DTOs
{
    public class InvitationDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string InvitedByName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class CreateInvitationDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        public int ExpiryDays { get; set; } = 7; // Default 7 days
    }

    public class InvitationCheckDto
    {
        public bool IsInvited { get; set; }
        public string? RoleName { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsExpired { get; set; }
    }
}
