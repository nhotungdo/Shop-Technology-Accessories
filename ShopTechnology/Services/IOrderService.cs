using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IOrderService
    {
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<PagedResult<Order>> GetOrdersAsync(OrderFilterViewModel filter, int page = 1, int pageSize = 20);
        Task<Order> CreateOrderAsync(CreateOrderViewModel model);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? notes = null);
        Task<bool> CancelOrderAsync(int orderId, string reason);
        Task<bool> AddOrderHistoryAsync(int orderId, string status, string? notes = null);
        Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(int orderId);
        Task<string> GenerateOrderNumberAsync();
        Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
        Task<OrderSummaryViewModel> GetOrderSummaryAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate);
    }
}
