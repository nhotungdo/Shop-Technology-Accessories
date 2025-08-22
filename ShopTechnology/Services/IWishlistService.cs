using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IWishlistService
{
    Task<List<Wishlist>> GetWishlistByUserIdAsync(Guid userId);
    Task<Wishlist?> GetWishlistItemAsync(Guid userId, int productId);
    Task<bool> AddToWishlistAsync(Guid userId, int productId);
    Task<bool> RemoveFromWishlistAsync(Guid userId, int productId);
    Task<bool> ClearWishlistAsync(Guid userId);
    Task<bool> IsInWishlistAsync(Guid userId, int productId);
    Task<int> GetWishlistCountAsync(Guid userId);
    Task<List<Product>> GetWishlistProductsAsync(Guid userId);
    Task<bool> MoveToCartAsync(Guid userId, int productId);
    Task<List<Wishlist>> GetWishlistWithProductInfoAsync(Guid userId);
    Task<bool> UpdateWishlistItemAsync(int wishlistId, DateTime? createdAt);
    Task<List<Wishlist>> GetRecentWishlistItemsAsync(Guid userId, int count = 5);
    Task<bool> RemoveExpiredWishlistItemsAsync(DateTime expirationDate);
}
