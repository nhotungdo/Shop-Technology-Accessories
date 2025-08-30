using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ConfirmEmailAsync(int userId, string token);
        Task<bool> IsEmailConfirmedAsync(int userId);
        Task<string> GetUserRoleAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, string fullName, string? phoneNumber, string? address);
        Task<bool> UpdateAvatarAsync(int userId, string avatarUrl);
    }
}
