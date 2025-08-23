using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(int? userId);
        Task<ServiceResult> AddToCartAsync(int? userId, int productId, int quantity);
        Task<ServiceResult> UpdateQuantityAsync(int? userId, int cartItemId, int quantity);
        Task<ServiceResult> RemoveFromCartAsync(int? userId, int cartItemId);
        Task<ServiceResult> ClearCartAsync(int? userId);
        Task<ServiceResult> ApplyPromotionAsync(int? userId, string promotionCode);
        Task<ServiceResult> RemovePromotionAsync(int? userId);
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
