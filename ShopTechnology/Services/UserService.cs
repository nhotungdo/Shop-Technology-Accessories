using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class UserService : IUserService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ShopTechnologyAccessoriesContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Orders)
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created: {Email}", user.Email);
        return user;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, User user)
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null)
            {
                return false;
            }

            existingUser.FullName = user.FullName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Address = user.Address;
            existingUser.City = user.City;
            existingUser.PostalCode = user.PostalCode;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User status toggled: {UserId} -> {IsActive}", userId, user.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, int roleId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                return false;
            }

            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User role changed: {UserId} -> {RoleName}", userId, role.RoleName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user role: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == roleName)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Users
            .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
    }

    public async Task<List<User>> GetRecentUsersAsync(int count = 10)
    {
        return await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Where(u => u.FullName.Contains(searchTerm) ||
                       u.Email.Contains(searchTerm) ||
                       u.PhoneNumber.Contains(searchTerm))
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> VerifyPasswordAsync(Guid userId, string passwordHash)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.PasswordHash == passwordHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(Guid userId, string fullName, string phoneNumber, string? address, string? city, string? postalCode)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
            user.City = city;
            user.PostalCode = postalCode;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User profile updated: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
            return false;
        }
    }
}
