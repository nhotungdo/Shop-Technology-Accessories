using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TopProductViewModel>> GetTopProductsAsync(int count = 10);
        Task<IEnumerable<TopCategoryViewModel>> GetTopCategoriesAsync(int count = 5);
        Task<CustomerAnalyticsViewModel> GetCustomerAnalyticsAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalCustomersAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<DailySalesViewModel>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
    }
}
