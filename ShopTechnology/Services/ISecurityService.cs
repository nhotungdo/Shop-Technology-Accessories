using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface ISecurityService
    {
        Task<bool> ValidatePasswordAsync(string password);
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<bool> ValidateJwtTokenAsync(string token);
        Task<string> GenerateEmailVerificationTokenAsync();
        Task<string> GeneratePasswordResetTokenAsync();
        Task<bool> IsUserLockedOutAsync(string email);
        Task IncrementFailedLoginAttemptsAsync(string email);
        Task ResetFailedLoginAttemptsAsync(string email);
        Task<bool> IsValidEmailAsync(string email);
        Task<bool> IsStrongPasswordAsync(string password);
    }
}
