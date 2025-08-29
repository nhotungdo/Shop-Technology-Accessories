using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface INotificationService
    {
        Task<bool> CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? linkUrl = null);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> SendOrderStatusNotificationAsync(string userId, string orderNumber, string status);
        Task<bool> SendPromotionNotificationAsync(string userId, string promotionTitle);
        Task<bool> SendStockAlertNotificationAsync(string userId, string productName);
    }
}
