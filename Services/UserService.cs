using Microsoft.EntityFrameworkCore;
using YopoAPI.Data;
using YopoAPI.DTOs;
using YopoAPI.Models;
using BC = BCrypt.Net.BCrypt;

namespace YopoAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string email);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<User?> ValidateUserAsync(string email, string password);
        Task<User?> ValidateUserByEmailOrPhoneAsync(string emailOrPhone, string password);
        Task<UserDto> CreateUserWithInvitationAsync(SignupDto signupDto, Invitation invitation);
        Task<bool> IsFirstUserAsync();
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<bool> HasRoleAsync(int userId, string roleName);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    RoleName = u.Role.Name
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Role = new RoleDto
                {
                    Id = user.Role.Id,
                    Name = user.Role.Name,
                    Description = user.Role.Description,
                    CreatedAt = user.Role.CreatedAt
                }
            };
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Role = new RoleDto
                {
                    Id = user.Role.Id,
                    Name = user.Role.Name,
                    Description = user.Role.Description,
                    CreatedAt = user.Role.CreatedAt
                }
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                PasswordHash = BC.HashPassword(createUserDto.Password),
                RoleId = createUserDto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id) ?? throw new InvalidOperationException("Failed to create user");
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Email = updateUserDto.Email;
            user.RoleId = updateUserDto.RoleId;
            user.IsActive = updateUserDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetUserByIdAsync(id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!BC.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BC.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || !BC.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User?> ValidateUserByEmailOrPhoneAsync(string emailOrPhone, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => (u.Email == emailOrPhone || u.PhoneNumber == emailOrPhone) && u.IsActive);

            if (user == null || !BC.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<UserDto> CreateUserWithInvitationAsync(SignupDto signupDto, Invitation invitation)
        {
            // Check if this is the first user (should be Super Admin)
            var isFirstUser = await IsFirstUserAsync();
            var roleId = isFirstUser ? 1 : invitation.RoleId; // 1 = Super Admin role

            var user = new User
            {
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Email = signupDto.Email ?? string.Empty,
                PhoneNumber = signupDto.PhoneNumber,
                PasswordHash = BC.HashPassword(signupDto.Password),
                RoleId = roleId,
                IsSuperAdmin = isFirstUser,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id) ?? throw new InvalidOperationException("Failed to create user");
        }

        public async Task<bool> IsFirstUserAsync()
        {
            return !await _context.Users.AnyAsync();
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.RoleId = 2; // Default to Normal User role
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AnyAsync(u => u.Id == userId && u.Role.Name == roleName);
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.PasswordHash = BC.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
