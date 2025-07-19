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

namespace YopoAPI.Modules.UserManagement.Services
{
    public interface IInvitationService
    {
        Task<IEnumerable<InvitationDto>> GetAllInvitationsAsync();
        Task<InvitationDto?> GetInvitationByIdAsync(int id);
        Task<InvitationDto> CreateInvitationAsync(CreateInvitationDto createInvitationDto, int invitedByUserId);
        Task<InvitationCheckDto> CheckInvitationAsync(string email);
        Task<Invitation?> GetValidInvitationAsync(string email);
        Task<bool> MarkInvitationAsUsedAsync(int invitationId);
        Task<bool> DeleteInvitationAsync(int id);
    }

    public class InvitationService : IInvitationService
    {
        private readonly ApplicationDbContext _context;

        public InvitationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvitationDto>> GetAllInvitationsAsync()
        {
            return await _context.Invitations
                .Include(i => i.InvitedBy)
                .Include(i => i.Role)
                .Select(i => new InvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    PhoneNumber = i.PhoneNumber,
                    InvitedByName = i.InvitedBy.FullName,
                    RoleName = i.Role.Name,
                    IsUsed = i.IsUsed,
                    ExpiresAt = i.ExpiresAt,
                    CreatedAt = i.CreatedAt,
                    UsedAt = i.UsedAt
                })
                .ToListAsync();
        }

        public async Task<InvitationDto?> GetInvitationByIdAsync(int id)
        {
            var invitation = await _context.Invitations
                .Include(i => i.InvitedBy)
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invitation == null) return null;

            return new InvitationDto
            {
                Id = invitation.Id,
                Email = invitation.Email,
                PhoneNumber = invitation.PhoneNumber,
                InvitedByName = invitation.InvitedBy.FullName,
                RoleName = invitation.Role.Name,
                IsUsed = invitation.IsUsed,
                ExpiresAt = invitation.ExpiresAt,
                CreatedAt = invitation.CreatedAt,
                UsedAt = invitation.UsedAt
            };
        }

        public async Task<InvitationDto> CreateInvitationAsync(CreateInvitationDto createInvitationDto, int invitedByUserId)
        {
            var invitation = new Invitation
            {
                Email = createInvitationDto.Email,
                PhoneNumber = createInvitationDto.PhoneNumber,
                InvitedByUserId = invitedByUserId,
                RoleId = createInvitationDto.RoleId,
                ExpiresAt = DateTime.UtcNow.AddDays(createInvitationDto.ExpiryDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            return await GetInvitationByIdAsync(invitation.Id) ?? throw new InvalidOperationException("Failed to create invitation");
        }

        public async Task<InvitationCheckDto> CheckInvitationAsync(string email)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Email == email && !i.IsUsed);

            if (invitation == null)
            {
                return new InvitationCheckDto
                {
                    IsInvited = false,
                    IsExpired = false
                };
            }

            bool isExpired = invitation.ExpiresAt < DateTime.UtcNow;

            return new InvitationCheckDto
            {
                IsInvited = true,
                RoleName = invitation.Role.Name,
                ExpiresAt = invitation.ExpiresAt,
                IsExpired = isExpired
            };
        }

        public async Task<Invitation?> GetValidInvitationAsync(string email)
        {
            return await _context.Invitations
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<bool> MarkInvitationAsUsedAsync(int invitationId)
        {
            var invitation = await _context.Invitations.FindAsync(invitationId);
            if (invitation == null) return false;

            invitation.IsUsed = true;
            invitation.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInvitationAsync(int id)
        {
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null) return false;

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}



