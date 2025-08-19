using ShopTechnology.DTOs;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services;

public interface IOrderService
{
    // Basic CRUD operations
    Task<List<OrderDTO>> GetAllOrdersAsync();
    Task<OrderDTO?> GetOrderByIdAsync(Guid orderId);
    Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto);
    Task<bool> DeleteOrderAsync(Guid orderId);

    // User-specific operations
    Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId);
    Task<(bool ok, Guid orderId, string message)> CreateOrderFromCartAsync(Guid userId, string shippingAddress, string paymentMethod);

    // Legacy methods for backward compatibility
    Task<List<OrderViewModel>> GetOrderHistoryAsync(Guid userId);
    Task<OrderViewModel?> GetOrderAsync(Guid orderId);

    // Order management
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);
    Task<bool> CancelOrderAsync(Guid orderId);

    // Special queries
    Task<List<OrderDTO>> GetRecentOrdersAsync(int count);
    Task<List<OrderDTO>> GetOrdersByStatusAsync(string status);
    Task<List<OrderDTO>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);

    // Statistics
    Task<int> GetTotalOrdersCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
}
