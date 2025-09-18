

namespace ShopTechnology.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailConfirmationAsync(string to, string callbackUrl);
        Task<bool> SendEmailVerificationAsync(string to, string userName, string token);
        Task<bool> SendPasswordResetAsync(string to, string callbackUrl);
        Task<bool> SendPasswordResetAsync(string to, string userName, string token);
        Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, string orderDetails);
        Task<bool> SendOrderStatusUpdateAsync(string to, string orderNumber, string status, string? trackingNumber = null);
        Task<bool> SendWelcomeEmailAsync(string to, string userName);
        Task<bool> SendPromotionEmailAsync(string to, string subject, string promotionDetails);
        Task<bool> SendStockAlertAsync(string to, string productName, int currentStock);
    }
}
