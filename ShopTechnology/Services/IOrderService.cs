using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IOrderService
    {
        Task<(bool ok, Guid orderId, string message)> CreateOrderFromCartAsync(Guid userId, string shippingAddress, string paymentMethod);
        Task<List<OrderViewModel>> GetOrderHistoryAsync(Guid userId);
        Task<OrderViewModel?> GetOrderAsync(Guid orderId);
    }
}
