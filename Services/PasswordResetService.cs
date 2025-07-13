using Microsoft.EntityFrameworkCore;
using YopoAPI.Data;
using YopoAPI.Models;

namespace YopoAPI.Services
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetCodeAsync(string email);
        Task<bool> VerifyResetCodeAsync(string email, string code);
        Task<bool> IsCodeValidAsync(string email, string code);
        Task<bool> MarkCodeAsUsedAsync(string email, string code);
        Task CleanupExpiredTokensAsync();
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateResetCodeAsync(string email)
        {
            // Generate a 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Remove any existing unused tokens for this email
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.Email == email && !t.IsUsed)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(existingTokens);

            // Create new token
            var token = new PasswordResetToken
            {
                Email = email,
                Token = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Token expires in 15 minutes
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task<bool> VerifyResetCodeAsync(string email, string code)
        {
            var token = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email && t.Token == code && !t.IsUsed);

            if (token == null) return false;

            // Check if token is expired
            if (token.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> IsCodeValidAsync(string email, string code)
        {
            return await VerifyResetCodeAsync(email, code);
        }

        public async Task<bool> MarkCodeAsUsedAsync(string email, string code)
        {
            var token = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email && t.Token == code && !t.IsUsed);

            if (token == null) return false;

            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}

