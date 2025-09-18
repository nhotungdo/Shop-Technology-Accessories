using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace ShopTechnology.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public UserService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Authentication and User Management
        public async Task<User?> RegisterUserAsync(string fullName, string email, string phoneNumber, string password, DateTime dateOfBirth)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                    return null;

                // Get default User role
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                    return null;

                // Generate email verification token
                var emailToken = GenerateSecureToken();
                var emailExpiry = DateTime.UtcNow.AddHours(24);

                var user = new User
                {
                    RoleId = userRole.RoleId,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Password = HashPassword(password),
                    DateOfBirth = dateOfBirth,
                    IsEmailVerified = false,
                    IsPhoneVerified = false,
                    EmailVerificationToken = emailToken,
                    EmailVerificationExpiry = emailExpiry,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send email verification
                await _emailService.SendEmailVerificationAsync(email, fullName, emailToken);

                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user == null || !VerifyPassword(password, user.Password))
                    return null;

                // Check if email is verified
                if (!user.IsEmailVerified)
                    return null;

                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null || user.EmailVerificationToken != token ||
                    user.EmailVerificationExpiry < DateTime.UtcNow)
                    return false;

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
                if (user == null)
                    return false;

                var resetToken = GenerateSecureToken();
                var resetExpiry = DateTime.UtcNow.AddHours(24);

                user.PasswordResetToken = resetToken;
                user.PasswordResetExpiry = resetExpiry;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send password reset email
                await _emailService.SendPasswordResetAsync(email, user.FullName, resetToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null || user.PasswordResetToken != token ||
                    user.PasswordResetExpiry < DateTime.UtcNow)
                    return false;

                user.Password = HashPassword(newPassword);
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UserUpdateModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;
                user.Province = model.Province;
                user.PostalCode = model.PostalCode;
                user.DateOfBirth = model.DateOfBirth;
                user.Avatar = model.Avatar;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<PagedResult<User>> GetAllUsersAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Email.Contains(searchTerm) || u.FullName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, int roleId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                var role = await _context.Roles.FindAsync(roleId);

                if (user == null || role == null)
                    return false;

                user.RoleId = roleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> SocialLoginAsync(string provider, string socialId, string email, string fullName)
        {
            try
            {
                // Check if user exists with this social login
                var existingUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.SocialLoginProvider == provider && u.SocialLoginId == socialId);

                if (existingUser != null)
                    return existingUser;

                // Check if user exists with this email
                var userByEmail = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (userByEmail != null)
                {
                    // Link social login to existing account
                    userByEmail.SocialLoginProvider = provider;
                    userByEmail.SocialLoginId = socialId;
                    userByEmail.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return userByEmail;
                }

                // Create new user
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                    return null;

                var newUser = new User
                {
                    RoleId = userRole.RoleId,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = "",
                    Password = "", // No password for social login
                    DateOfBirth = DateTime.Now.AddYears(-18),
                    IsEmailVerified = true, // Social logins are pre-verified
                    IsPhoneVerified = false,
                    SocialLoginProvider = provider,
                    SocialLoginId = socialId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return newUser;
            }
            catch
            {
                return null;
            }
        }

        // Role Management
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role?> CreateRoleAsync(string name)
        {
            try
            {
                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
                if (existingRole != null)
                    return null;

                var role = new Role
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return role;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateRoleAsync(int roleId, string name)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                    return false;

                role.Name = name;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                    return false;

                // Check if role is in use
                var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == roleId);
                if (usersWithRole)
                    return false;

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        private string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.Password != currentPassword) return false;

                user.Password = newPassword; // In real app, this should be hashed
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return false;

                var token = Guid.NewGuid().ToString();
                user.PasswordResetToken = token;
                user.PasswordResetExpiry = DateTime.UtcNow.AddHours(24);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(int userId, string token)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.EmailVerificationToken != token) return false;

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsEmailConfirmedAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.IsEmailVerified ?? false;
        }

        public async Task<string> GetUserRoleAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            return user?.Role?.Name ?? string.Empty;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? address)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.FullName = fullName;
                user.PhoneNumber = phoneNumber ?? string.Empty;
                user.Address = address;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAvatarAsync(int userId, string avatarUrl)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Avatar = avatarUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
