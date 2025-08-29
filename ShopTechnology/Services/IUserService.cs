using ShopTechnology.Models;
using Microsoft.AspNetCore.Identity;

namespace ShopTechnology.Services
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<bool> IsInRoleAsync(string userId, string roleName);
        Task<bool> UpdateProfileAsync(string userId, string firstName, string lastName, string? phoneNumber, string? address);
        Task<bool> UpdateAvatarAsync(string userId, string avatarUrl);
    }
}
