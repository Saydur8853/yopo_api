using Microsoft.EntityFrameworkCore;
using YopoAPI.Data;
using YopoAPI.Modules.Authentication.DTOs;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.RoleManagement.DTOs;
using YopoAPI.Modules.PolicyManagement.DTOs;
using YopoAPI.Modules.Authentication.Models;
using YopoAPI.Modules.UserManagement.Models;
using YopoAPI.Modules.RoleManagement.Models;
using YopoAPI.Modules.PolicyManagement.Models;

namespace YopoAPI.Modules.RoleManagement.Services
{
    public interface IPrivilegeService
    {
        Task<IEnumerable<PrivilegeDto>> GetAllPrivilegesAsync();
        Task<PrivilegeDto?> GetPrivilegeByIdAsync(int id);
        Task<PrivilegeDto> CreatePrivilegeAsync(CreatePrivilegeDto createPrivilegeDto);
        Task<PrivilegeDto?> UpdatePrivilegeAsync(int id, UpdatePrivilegeDto updatePrivilegeDto);
        Task<bool> DeletePrivilegeAsync(int id);
        Task<bool> AssignPrivilegesToRoleAsync(int roleId, List<int> privilegeIds);
        Task<bool> RemovePrivilegeFromRoleAsync(int roleId, int privilegeId);
        Task<IEnumerable<PrivilegeDto>> GetRolePrivilegesAsync(int roleId);
        Task<bool> SetRoleHierarchyAsync(int roleId, int? parentRoleId, int hierarchyLevel);
    }

    public class PrivilegeService : IPrivilegeService
    {
        private readonly ApplicationDbContext _context;

        public PrivilegeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PrivilegeDto>> GetAllPrivilegesAsync()
        {
            return await _context.Privileges
                .Select(p => new PrivilegeDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<PrivilegeDto?> GetPrivilegeByIdAsync(int id)
        {
            var privilege = await _context.Privileges.FindAsync(id);
            if (privilege == null) return null;

            return new PrivilegeDto
            {
                Id = privilege.Id,
                Name = privilege.Name,
                Description = privilege.Description,
                Category = privilege.Category,
                CreatedAt = privilege.CreatedAt
            };
        }

        public async Task<PrivilegeDto> CreatePrivilegeAsync(CreatePrivilegeDto createPrivilegeDto)
        {
            var privilege = new Privilege
            {
                Name = createPrivilegeDto.Name,
                Description = createPrivilegeDto.Description,
                Category = createPrivilegeDto.Category,
                CreatedAt = DateTime.UtcNow
            };

            _context.Privileges.Add(privilege);
            await _context.SaveChangesAsync();

            return new PrivilegeDto
            {
                Id = privilege.Id,
                Name = privilege.Name,
                Description = privilege.Description,
                Category = privilege.Category,
                CreatedAt = privilege.CreatedAt
            };
        }

        public async Task<PrivilegeDto?> UpdatePrivilegeAsync(int id, UpdatePrivilegeDto updatePrivilegeDto)
        {
            var privilege = await _context.Privileges.FindAsync(id);
            if (privilege == null) return null;

            privilege.Name = updatePrivilegeDto.Name;
            privilege.Description = updatePrivilegeDto.Description;
            privilege.Category = updatePrivilegeDto.Category;

            await _context.SaveChangesAsync();

            return new PrivilegeDto
            {
                Id = privilege.Id,
                Name = privilege.Name,
                Description = privilege.Description,
                Category = privilege.Category,
                CreatedAt = privilege.CreatedAt
            };
        }

        public async Task<bool> DeletePrivilegeAsync(int id)
        {
            var privilege = await _context.Privileges.FindAsync(id);
            if (privilege == null) return false;

            _context.Privileges.Remove(privilege);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignPrivilegesToRoleAsync(int roleId, List<int> privilegeIds)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;

            // Remove existing privileges for this role
            var existingRolePrivileges = await _context.RolePrivileges
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePrivileges.RemoveRange(existingRolePrivileges);

            // Add new privileges
            var newRolePrivileges = privilegeIds.Select(privilegeId => new RolePrivilege
            {
                RoleId = roleId,
                PrivilegeId = privilegeId,
                AssignedAt = DateTime.UtcNow
            });

            _context.RolePrivileges.AddRange(newRolePrivileges);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemovePrivilegeFromRoleAsync(int roleId, int privilegeId)
        {
            var rolePrivilege = await _context.RolePrivileges
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == privilegeId);

            if (rolePrivilege == null) return false;

            _context.RolePrivileges.Remove(rolePrivilege);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PrivilegeDto>> GetRolePrivilegesAsync(int roleId)
        {
            return await _context.RolePrivileges
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Privilege)
                .Select(rp => new PrivilegeDto
                {
                    Id = rp.Privilege.Id,
                    Name = rp.Privilege.Name,
                    Description = rp.Privilege.Description,
                    Category = rp.Privilege.Category,
                    CreatedAt = rp.Privilege.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> SetRoleHierarchyAsync(int roleId, int? parentRoleId, int hierarchyLevel)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return false;

            role.ParentRoleId = parentRoleId;
            role.HierarchyLevel = hierarchyLevel;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}



