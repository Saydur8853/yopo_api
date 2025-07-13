using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string Token { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UsedAt { get; set; }
    }
}

