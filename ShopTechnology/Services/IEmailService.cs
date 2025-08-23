using ShopTechnology.DTOs;

namespace ShopTechnology.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string fullName);
        Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink);
        Task SendOrderConfirmationEmailAsync(string email, string fullName, string orderNumber);
        Task SendOrderStatusUpdateEmailAsync(string email, string fullName, string orderNumber, string status);
    }
}
