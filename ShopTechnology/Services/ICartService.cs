using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface ICartService
{
    Task<Cart?> GetCartByUserIdAsync(Guid userId);
    Task<bool> AddToCartAsync(Guid userId, int productId, int quantity);
    Task<bool> RemoveFromCartAsync(Guid userId, int cartItemId);
    Task<bool> UpdateCartItemQuantityAsync(Guid userId, int cartItemId, int quantity);
    Task<bool> ClearCartAsync(Guid userId);
    Task<int> GetCartItemCountAsync(Guid userId);
    Task<decimal> GetCartTotalAsync(Guid userId);
    Task<bool> IsProductInCartAsync(Guid userId, int productId);
    Task<CartItem?> GetCartItemAsync(Guid userId, int productId);
    Task<List<CartItem>> GetCartItemsAsync(Guid userId);
    Task<bool> ValidateCartAsync(Guid userId);
}
