using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IOrderService
    {
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> UpdatePaymentStatusAsync(int orderId, string status);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountAsync();
        Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetOrdersByStatusAsync(string status);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
