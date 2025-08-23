using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync(int page = 1, int pageSize = 20);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> IsEmailExistsAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string newPassword);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> ToggleUserStatusAsync(int userId);
    Task<bool> AssignRoleAsync(int userId, int roleId);
    Task<bool> RemoveRoleAsync(int userId, int roleId);
    Task<List<Role>> GetUserRolesAsync(int userId);
    Task<bool> HasRoleAsync(int userId, string roleName);
    Task<List<User>> GetUsersByRoleAsync(string roleName);
    Task<List<User>> GetActiveUsersAsync();
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate);
    Task<List<User>> GetRecentUsersAsync(int count = 10);
    Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task<bool> UpdateUserProfileAsync(int userId, string fullName, string phoneNumber, string? address, string? city, string? province, string? postalCode);
    Task<bool> VerifyEmailAsync(int userId);
    Task<bool> SetPasswordResetTokenAsync(string email, string token, DateTime expiry);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
