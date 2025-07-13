using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoAPI.DTOs;
using YopoAPI.Services;

namespace YopoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Super Admin")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        [HttpPost]
        public async Task<ActionResult<InvitationDto>> CreateInvitation(CreateInvitationDto createInvitationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var invitation = await _invitationService.CreateInvitationAsync(createInvitationDto, currentUserId);

            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, invitation);
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin,Property Admin,Security Admin")]
        public async Task<ActionResult<IEnumerable<InvitationDto>>> GetInvitations()
        {
            var invitations = await _invitationService.GetAllInvitationsAsync();
            return Ok(invitations);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Super Admin,Property Admin,Security Admin")]
        public async Task<ActionResult<InvitationDto>> GetInvitation(int id)
        {
            var invitation = await _invitationService.GetInvitationByIdAsync(id);
            if (invitation == null)
                return NotFound(new { message = "Invitation not found" });

            return Ok(invitation);
        }

        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<ActionResult<InvitationCheckDto>> CheckInvitation([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email is required" });

            var invitationCheck = await _invitationService.CheckInvitationAsync(email);
            return Ok(invitationCheck);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInvitation(int id)
        {
            var result = await _invitationService.DeleteInvitationAsync(id);
            if (!result)
                return NotFound(new { message = "Invitation not found" });

            return NoContent();
        }
    }
}

