using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IBackgroundJobService
    {
        Task InitializeJobsAsync();
        Task ScheduleOrderStatusUpdateAsync(int orderId, string status, DateTime scheduledTime);
        Task ScheduleEmailReminderAsync(int userId, string emailType, DateTime scheduledTime);
        Task ScheduleStockAlertAsync(int productId, DateTime scheduledTime);
        Task SchedulePromotionExpiryAsync(int promotionId, DateTime expiryTime);
        Task ScheduleDataCleanupAsync(DateTime scheduledTime);
        Task ScheduleAnalyticsReportAsync(DateTime scheduledTime);
        Task CancelJobAsync(string jobId);
        Task<List<JobInfo>> GetScheduledJobsAsync();
    }

    public class JobInfo
    {
        public string JobId { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
