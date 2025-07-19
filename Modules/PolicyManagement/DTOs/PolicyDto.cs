using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Modules.PolicyManagement.DTOs
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePolicyDto
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(20)]
        public string Version { get; set; } = "1.0";
    }

    public class UpdatePolicyDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(20)]
        public string Version { get; set; } = string.Empty;
    }
}


