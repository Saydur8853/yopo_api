using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoAPI.Modules.Authentication.DTOs;
using YopoAPI.Modules.UserManagement.DTOs;
using YopoAPI.Modules.RoleManagement.DTOs;
using YopoAPI.Modules.PolicyManagement.DTOs;
using YopoAPI.Modules.Authentication.Services;
using YopoAPI.Modules.UserManagement.Services;
using YopoAPI.Modules.RoleManagement.Services;
using YopoAPI.Modules.PolicyManagement.Services;

namespace YopoAPI.Modules.UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userService.UserExistsAsync(createUserDto.Email))
                return BadRequest(new { message = "User with this email already exists" });

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found" });

            // Check if email is being changed and if it already exists
            if (existingUser.Email != updateUserDto.Email && await _userService.UserExistsAsync(updateUserDto.Email))
                return BadRequest(new { message = "User with this email already exists" });

            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return BadRequest(new { message = "Failed to delete user" });

            return NoContent();
        }

        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpPost("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleUserStatus(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var updateUserDto = new UpdateUserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.Role.Id,
                IsActive = !user.IsActive
            };

            await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(new { message = $"User status changed to {(updateUserDto.IsActive ? "Active" : "Inactive")}" });
        }

        [HttpPost("{id}/assign-role")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> AssignRole(int id, AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userService.AssignRoleToUserAsync(id, assignRoleDto.RoleId);
            if (!result)
                return BadRequest(new { message = "Failed to assign role" });

            return Ok(new { message = "Role assigned successfully" });
        }

        [HttpPost("{id}/update-status")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> UpdateStatus(int id, UpdateUserStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userService.UpdateUserStatusAsync(id, updateStatusDto.IsActive);
            if (!result)
                return BadRequest(new { message = "Failed to update user status" });

            return Ok(new { message = $"User status updated to {(updateStatusDto.IsActive ? "Active" : "Inactive")}" });
        }

        [HttpDelete("{id}/remove-role")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> RemoveRole(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userService.RemoveRoleFromUserAsync(id);
            if (!result)
                return BadRequest(new { message = "Failed to remove role" });

            return Ok(new { message = "Role removed successfully, user assigned to Normal User role" });
        }
    }
}



