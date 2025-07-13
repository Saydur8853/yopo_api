using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoAPI.DTOs;
using YopoAPI.Services;

namespace YopoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Super Admin")]
    public class PrivilegesController : ControllerBase
    {
        private readonly IPrivilegeService _privilegeService;

        public PrivilegesController(IPrivilegeService privilegeService)
        {
            _privilegeService = privilegeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrivilegeDto>>> GetPrivileges()
        {
            var privileges = await _privilegeService.GetAllPrivilegesAsync();
            return Ok(privileges);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrivilegeDto>> GetPrivilege(int id)
        {
            var privilege = await _privilegeService.GetPrivilegeByIdAsync(id);
            if (privilege == null)
                return NotFound(new { message = "Privilege not found" });

            return Ok(privilege);
        }

        [HttpPost]
        public async Task<ActionResult<PrivilegeDto>> CreatePrivilege(CreatePrivilegeDto createPrivilegeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var privilege = await _privilegeService.CreatePrivilegeAsync(createPrivilegeDto);
            return CreatedAtAction(nameof(GetPrivilege), new { id = privilege.Id }, privilege);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PrivilegeDto>> UpdatePrivilege(int id, UpdatePrivilegeDto updatePrivilegeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var privilege = await _privilegeService.UpdatePrivilegeAsync(id, updatePrivilegeDto);
            if (privilege == null)
                return NotFound(new { message = "Privilege not found" });

            return Ok(privilege);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePrivilege(int id)
        {
            var result = await _privilegeService.DeletePrivilegeAsync(id);
            if (!result)
                return NotFound(new { message = "Privilege not found" });

            return NoContent();
        }
    }
}

