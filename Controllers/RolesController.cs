using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoAPI.DTOs;
using YopoAPI.Services;

namespace YopoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Super Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IPrivilegeService _privilegeService;

        public RolesController(IRoleService roleService, IPrivilegeService privilegeService)
        {
            _roleService = roleService;
            _privilegeService = privilegeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _roleService.RoleExistsAsync(createRoleDto.Name))
                return BadRequest(new { message = "Role with this name already exists" });

            var role = await _roleService.CreateRoleAsync(createRoleDto);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoleDto>> UpdateRole(int id, UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingRole = await _roleService.GetRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { message = "Role not found" });

            var updatedRole = await _roleService.UpdateRoleAsync(id, updateRoleDto);
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            if (await _roleService.RoleHasUsersAsync(id))
                return BadRequest(new { message = "Cannot delete a role that has users assigned" });

            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
                return BadRequest(new { message = "Failed to delete role" });

            return NoContent();
        }

        [HttpPost("{id}/assign-privileges")]
        public async Task<ActionResult> AssignPrivilegesToRole(int id, AssignPrivilegesToRoleDto assignPrivilegesDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var result = await _privilegeService.AssignPrivilegesToRoleAsync(id, assignPrivilegesDto.PrivilegeIds);
            if (!result)
                return BadRequest(new { message = "Failed to assign privileges" });

            return Ok(new { message = "Privileges assigned successfully" });
        }

        [HttpGet("{id}/privileges")]
        public async Task<ActionResult<IEnumerable<PrivilegeDto>>> GetRolePrivileges(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var privileges = await _privilegeService.GetRolePrivilegesAsync(id);
            return Ok(privileges);
        }

        [HttpPost("hierarchy")]
        public async Task<ActionResult> SetRoleHierarchy(RoleHierarchyDto roleHierarchyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _privilegeService.SetRoleHierarchyAsync(
                roleHierarchyDto.RoleId, 
                roleHierarchyDto.ParentRoleId, 
                roleHierarchyDto.HierarchyLevel);

            if (!result)
                return BadRequest(new { message = "Failed to set role hierarchy" });

            return Ok(new { message = "Role hierarchy set successfully" });
        }
    }
}

