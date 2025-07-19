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

namespace YopoAPI.Modules.PolicyManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet("terms")]
        [AllowAnonymous]
        public async Task<ActionResult<PolicyDto>> GetTerms()
        {
            var policy = await _policyService.GetPolicyByTypeAsync("terms");
            if (policy == null)
                return NotFound(new { message = "Terms and conditions not found" });

            return Ok(policy);
        }

        [HttpGet("privacy")]
        [AllowAnonymous]
        public async Task<ActionResult<PolicyDto>> GetPrivacyPolicy()
        {
            var policy = await _policyService.GetPolicyByTypeAsync("privacy");
            if (policy == null)
                return NotFound(new { message = "Privacy policy not found" });

            return Ok(policy);
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult<IEnumerable<PolicyDto>>> GetAllPolicies()
        {
            var policies = await _policyService.GetAllPoliciesAsync();
            return Ok(policies);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult<PolicyDto>> CreatePolicy(CreatePolicyDto createPolicyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _policyService.CreatePolicyAsync(createPolicyDto);
            return Ok(policy);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult<PolicyDto>> UpdatePolicy(int id, UpdatePolicyDto updatePolicyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _policyService.UpdatePolicyAsync(id, updatePolicyDto);
            if (policy == null)
                return NotFound(new { message = "Policy not found" });

            return Ok(policy);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> DeletePolicy(int id)
        {
            var result = await _policyService.DeletePolicyAsync(id);
            if (!result)
                return NotFound(new { message = "Policy not found" });

            return NoContent();
        }
    }
}



