using System.ComponentModel.DataAnnotations;
using YopoAPI.Modules.RoleManagement.Models;

namespace YopoAPI.Modules.UserManagement.Models
{
    public class Invitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public int InvitedByUserId { get; set; }
        public virtual User InvitedBy { get; set; } = null!;

        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public bool IsUsed { get; set; } = false;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UsedAt { get; set; }
    }
}



