using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetLink);
    Task<bool> SendOrderConfirmationEmailAsync(string email, string orderNumber, decimal totalAmount);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);
    Task<bool> SendOrderStatusUpdateEmailAsync(string email, string orderNumber, string status);
}
