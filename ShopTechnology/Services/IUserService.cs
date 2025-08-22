using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> IsEmailExistsAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(Guid userId, User user);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash);
    Task<bool> UpdateLastLoginAsync(Guid userId);
    Task<bool> ToggleUserStatusAsync(Guid userId);
    Task<bool> ChangeUserRoleAsync(Guid userId, int roleId);
    Task<List<User>> GetUsersByRoleAsync(string roleName);
    Task<List<User>> GetActiveUsersAsync();
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetNewUsersCountAsync(DateTime startDate, DateTime endDate);
    Task<List<User>> GetRecentUsersAsync(int count = 10);
    Task<List<User>> SearchUsersAsync(string searchTerm);
    Task<bool> VerifyPasswordAsync(Guid userId, string passwordHash);
    Task<bool> UpdateUserProfileAsync(Guid userId, string fullName, string phoneNumber, string? address, string? city, string? postalCode);
}
