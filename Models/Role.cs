using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public int? ParentRoleId { get; set; }
        public virtual Role? ParentRole { get; set; }

        public int HierarchyLevel { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Role> SubRoles { get; set; } = new List<Role>();
        public virtual ICollection<RolePrivilege> RolePrivileges { get; set; } = new List<RolePrivilege>();
    }
}

