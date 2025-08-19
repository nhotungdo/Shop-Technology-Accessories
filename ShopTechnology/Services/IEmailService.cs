using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IEmailService
{
    Task<bool> SendOrderConfirmationAsync(OrderDTO order);
    Task<bool> SendOrderStatusUpdateAsync(OrderDTO order, string newStatus);
    Task<bool> SendPasswordResetAsync(string email, string resetToken);
    Task<bool> SendWelcomeEmailAsync(UserDTO user);
    Task<bool> SendPromotionEmailAsync(string email, string subject, string body);
    Task<bool> SendReviewReminderAsync(OrderDTO order);
    Task<bool> SendLowStockAlertAsync(ProductDTO product);
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}
