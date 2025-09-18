using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IUserService
    {
        // Authentication and User Management
        Task<User?> RegisterUserAsync(string fullName, string email, string phoneNumber, string password, DateTime dateOfBirth);
        Task<User?> LoginUserAsync(string email, string password);
        Task<bool> VerifyEmailAsync(string email, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> UpdateUserProfileAsync(int userId, UserUpdateModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task<PagedResult<User>> GetAllUsersAsync(int page, int pageSize, string? searchTerm = null);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ChangeUserRoleAsync(int userId, int roleId);
        Task<User?> SocialLoginAsync(string provider, string socialId, string email, string fullName);

        // Role Management
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> CreateRoleAsync(string name);
        Task<bool> UpdateRoleAsync(int roleId, string name);
        Task<bool> DeleteRoleAsync(int roleId);

        // Legacy methods for backward compatibility
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ConfirmEmailAsync(int userId, string token);
        Task<bool> IsEmailConfirmedAsync(int userId);
        Task<string> GetUserRoleAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? address);
        Task<bool> UpdateAvatarAsync(int userId, string avatarUrl);
    }
}
