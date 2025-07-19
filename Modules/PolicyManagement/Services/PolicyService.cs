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

namespace YopoAPI.Modules.PolicyManagement.Services
{
    public interface IPolicyService
    {
        Task<PolicyDto?> GetPolicyByTypeAsync(string type);
        Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto);
        Task<PolicyDto?> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto);
        Task<bool> DeletePolicyAsync(int id);
        Task<IEnumerable<PolicyDto>> GetAllPoliciesAsync();
    }

    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyDto?> GetPolicyByTypeAsync(string type)
        {
            var policy = await _context.Policies
                .FirstOrDefaultAsync(p => p.Type == type && p.IsActive);

            if (policy == null) return null;

            return new PolicyDto
            {
                Id = policy.Id,
                Type = policy.Type,
                Content = policy.Content,
                Version = policy.Version,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };
        }

        public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto)
        {
            // Deactivate existing policies of the same type
            var existingPolicies = await _context.Policies
                .Where(p => p.Type == createPolicyDto.Type)
                .ToListAsync();

            foreach (var existingPolicy in existingPolicies)
            {
                existingPolicy.IsActive = false;
            }

            var policy = new Policy
            {
                Type = createPolicyDto.Type,
                Content = createPolicyDto.Content,
                Version = createPolicyDto.Version,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            return new PolicyDto
            {
                Id = policy.Id,
                Type = policy.Type,
                Content = policy.Content,
                Version = policy.Version,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };
        }

        public async Task<PolicyDto?> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return null;

            policy.Content = updatePolicyDto.Content;
            policy.Version = updatePolicyDto.Version;
            policy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PolicyDto
            {
                Id = policy.Id,
                Type = policy.Type,
                Content = policy.Content,
                Version = policy.Version,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt
            };
        }

        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null) return false;

            _context.Policies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PolicyDto>> GetAllPoliciesAsync()
        {
            return await _context.Policies
                .Select(p => new PolicyDto
                {
                    Id = p.Id,
                    Type = p.Type,
                    Content = p.Content,
                    Version = p.Version,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();
        }
    }
}



