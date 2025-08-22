using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IUserService
{
    Task<UserDTO?> GetUserByIdAsync(Guid userId);
    Task<UserDTO?> GetUserByEmailAsync(string email);
    Task<List<UserDTO>> GetAllUsersAsync();
    Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto);
    Task<UserDTO> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDto);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO changePasswordDto);
    Task<bool> ValidateUserAsync(string email, string password);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsUserAdminAsync(Guid userId);
    Task<List<UserDTO>> GetUsersByRoleAsync(string roleName);
    Task<int> GetTotalUsersCountAsync();
    Task<bool> FixPasswordHashesAsync();
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> ValidateResetTokenAsync(string email, string token);

    Task<bool> CreateAdminUserAsync();
    Task<bool> CreateRolesAsync();
}
