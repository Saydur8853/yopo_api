using System.ComponentModel.DataAnnotations;

namespace YopoAPI.DTOs
{
    public class PrivilegeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePrivilegeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
    }

    public class UpdatePrivilegeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
    }

    public class AssignPrivilegesToRoleDto
    {
        [Required]
        public List<int> PrivilegeIds { get; set; } = new List<int>();
    }

    public class RoleHierarchyDto
    {
        [Required]
        public int RoleId { get; set; }

        public int? ParentRoleId { get; set; }

        [Required]
        public int HierarchyLevel { get; set; }
    }
}
