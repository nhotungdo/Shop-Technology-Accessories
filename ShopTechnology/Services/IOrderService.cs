using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order> CreateOrderAsync(CreateOrderViewModel model);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);
    Task<bool> CancelOrderAsync(Guid orderId);
    Task<List<Order>> GetOrdersByStatusAsync(string status);
    Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Order>> GetRecentOrdersAsync(int count = 10);
}
