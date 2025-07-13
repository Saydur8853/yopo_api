using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Models
{
    public class RolePrivilege
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public int PrivilegeId { get; set; }
        public virtual Privilege Privilege { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

