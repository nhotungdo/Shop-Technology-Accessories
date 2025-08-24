using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class UserService : IUserService
{
    private readonly ShopTechnologyAccessoriesContext _context;

    public UserService(ShopTechnologyAccessoriesContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => true /* u.IsActive - removed because column doesn't exist */)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.Now;
        // user.IsActive - removed because column doesn't exist = true;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.UserId);
        if (existingUser == null) return false;

        existingUser.FullName = user.FullName;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.Address = user.Address;
        existingUser.City = user.City;
        existingUser.Province = user.Province;
        existingUser.PostalCode = user.PostalCode;
        existingUser.DateOfBirth = user.DateOfBirth;
        existingUser.Avatar = user.Avatar;
        existingUser.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // user.IsActive - removed because column doesn't exist = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.Password = newPassword; // Trong thực tế nên hash password
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleUserStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // user.IsActive - removed because column doesn't exist = !// user.IsActive - removed because column doesn't exist;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, int roleId)
    {
        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingUserRole != null) return true; // Đã có role

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.Now
            // IsActive removed - column doesn't exist in database
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Role>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && true /* ur.IsActive - removed because column doesn't exist */)
            .Select(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && true /* ur.IsActive - removed because column doesn't exist */);
    }

    public async Task<List<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => true /* u.IsActive - removed because column doesn't exist */ && u.UserRoles.Any(ur => ur.Role.Name == roleName && true /* ur.IsActive - removed because column doesn't exist */))
            .ToListAsync();
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => true /* u.IsActive - removed because column doesn't exist */)
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => true /* u.IsActive - removed because column doesn't exist */);
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => true /* u.IsActive - removed because column doesn't exist */);
    }

    public async Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Users
            .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
    }

    public async Task<List<User>> GetRecentUsersAsync(int count = 10)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => true /* u.IsActive - removed because column doesn't exist */)
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => true /* u.IsActive - removed because column doesn't exist */ &&
                       (u.FullName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        u.PhoneNumber.Contains(searchTerm)))
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> VerifyPasswordAsync(int userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        return user.Password == password; // Trong thực tế nên so sánh hash
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, string fullName, string phoneNumber, string? address, string? city, string? province, string? postalCode)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.Address = address;
        user.City = city;
        user.Province = province;
        user.PostalCode = postalCode;
        user.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyEmailAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPasswordResetTokenAsync(string email, string token, DateTime expiry)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return false;

        user.PasswordResetToken = token;
        user.PasswordResetExpiry = expiry;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                     u.PasswordResetExpiry > DateTime.Now);
        if (user == null) return false;

        user.Password = newPassword; // Trong thực tế nên hash password
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }
}
