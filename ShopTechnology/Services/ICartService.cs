using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(string userId, string? sessionId = null);
        Task<Cart> CreateCartAsync(string userId, string? sessionId = null);
        Task<bool> AddToCartAsync(string userId, int productId, int quantity, string? sessionId = null);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int cartId);
        Task<CartViewModel> GetCartViewModelAsync(string userId, string? sessionId = null);
        Task<bool> MergeGuestCartAsync(string userId, string sessionId);
        Task<int> GetCartItemCountAsync(string userId, string? sessionId = null);
        Task<bool> IsProductInCartAsync(string userId, int productId, string? sessionId = null);
        Task<(bool Success, string Message)> ApplyPromoCodeAsync(string userId, string promoCode, string? sessionId = null);
    }
}
