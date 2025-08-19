using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface ICartService
{
    // Cart operations
    Task<CartDTO?> GetCartByUserIdAsync(Guid userId);
    Task<CartDTO> CreateCartAsync(Guid userId);
    Task<bool> ClearCartAsync(Guid userId);
    
    // Cart item operations
    Task<bool> AddToCartAsync(Guid userId, AddToCartDTO addToCartDto);
    Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveFromCartAsync(int cartItemId);
    Task<bool> RemoveAllFromCartAsync(Guid userId);
    
    // Cart queries
    Task<int> GetCartItemCountAsync(Guid userId);
    Task<decimal> GetCartTotalAsync(Guid userId);
    Task<bool> IsCartEmptyAsync(Guid userId);
    
    // Validation
    Task<bool> ValidateCartItemAsync(int productId, int quantity);
    Task<List<CartItemDTO>> GetInvalidCartItemsAsync(Guid userId);
}
