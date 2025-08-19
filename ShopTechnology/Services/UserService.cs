using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class UserService : IUserService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public UserService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        return _mapper.Map<UserDTO>(user);
    }

    public async Task<List<UserDTO>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return _mapper.Map<List<UserDTO>>(users);
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto)
    {
        // Check if email already exists
        if (await IsEmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Hash password (in production, use proper password hashing)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = passwordHash;
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task<UserDTO> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _mapper.Map(updateUserDto, user);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(userId) ?? throw new InvalidOperationException("Failed to update user");
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO changePasswordDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return false;
            }

            // Kiểm tra password hash có hợp lệ không
            if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2a$"))
            {
                // Nếu password hash không đúng format BCrypt, thử so sánh trực tiếp
                return user.PasswordHash == password;
            }

            // Sử dụng BCrypt để verify password
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            Console.WriteLine($"Error validating user {email}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUserAdminAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role?.RoleName == "Admin";
    }

    public async Task<List<UserDTO>> GetUsersByRoleAsync(string roleName)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == roleName)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return _mapper.Map<List<UserDTO>>(users);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<bool> FixPasswordHashesAsync()
    {
        try
        {
            var users = await _context.Users.ToListAsync();
            bool hasChanges = false;

            foreach (var user in users)
            {
                // Kiểm tra nếu password hash không phải BCrypt format
                if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2a$"))
                {
                    // Hash password hiện tại bằng BCrypt
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash ?? "123456");
                    user.UpdatedAt = DateTime.UtcNow;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }

            return hasChanges;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing password hashes: {ex.Message}");
            return false;
        }
    }

    // Password reset methods
    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            // Generate reset token
            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Save reset token
            var passwordReset = new PasswordReset
            {
                Email = email,
                Token = token,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            // Send email (if email service is available)
            try
            {
                // This would require injecting IEmailService
                // await _emailService.SendPasswordResetEmailAsync(email, token);
            }
            catch
            {
                // Continue even if email fails
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ForgotPasswordAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            var resetRecord = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email == email && pr.Token == token && !pr.IsUsed && pr.ExpiresAt > DateTime.UtcNow);

            if (resetRecord == null)
            {
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as used
            resetRecord.IsUsed = true;
            resetRecord.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ResetPasswordAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ValidateResetTokenAsync(string email, string token)
    {
        try
        {
            var resetRecord = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email == email && pr.Token == token && !pr.IsUsed && pr.ExpiresAt > DateTime.UtcNow);

            return resetRecord != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ValidateResetTokenAsync: {ex.Message}");
            return false;
        }
    }

    // External login methods
    public async Task<UserDTO?> GetUserByExternalLoginAsync(string provider, string providerKey)
    {
        try
        {
            var externalLogin = await _context.ExternalLogins
                .Include(el => el.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(el => el.Provider == provider && el.ProviderKey == providerKey);

            if (externalLogin == null)
            {
                return null;
            }

            // Update last login
            externalLogin.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDTO>(externalLogin.User);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserByExternalLoginAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<UserDTO> CreateUserFromExternalLoginAsync(string provider, string providerKey, string email, string name, string? pictureUrl)
    {
        try
        {
            // Create new user
            var user = new User
            {
                Email = email,
                FullName = name,
                RoleId = 2, // Default to User role
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create external login record
            var externalLogin = new ExternalLogin
            {
                UserId = user.UserId,
                Provider = provider,
                ProviderKey = providerKey,
                Email = email,
                Name = name,
                PictureUrl = pictureUrl,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.ExternalLogins.Add(externalLogin);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Failed to create user");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateUserFromExternalLoginAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> LinkExternalLoginAsync(Guid userId, string provider, string providerKey, string email, string name, string? pictureUrl)
    {
        try
        {
            var externalLogin = new ExternalLogin
            {
                UserId = userId,
                Provider = provider,
                ProviderKey = providerKey,
                Email = email,
                Name = name,
                PictureUrl = pictureUrl,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.ExternalLogins.Add(externalLogin);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LinkExternalLoginAsync: {ex.Message}");
            return false;
        }
    }
}
