using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoAPI.DTOs;
using YopoAPI.Services;

namespace YopoAPI.Controllers
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

