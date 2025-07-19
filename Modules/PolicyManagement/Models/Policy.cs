using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Modules.PolicyManagement.Models
{
    public class Policy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // "terms", "privacy"

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(20)]
        public string Version { get; set; } = "1.0";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}



