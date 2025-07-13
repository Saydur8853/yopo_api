using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Models
{
    public class Privilege
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<RolePrivilege> RolePrivileges { get; set; } = new List<RolePrivilege>();
    }
}

