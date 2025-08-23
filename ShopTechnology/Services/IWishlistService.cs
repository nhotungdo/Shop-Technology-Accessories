using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IWishlistService
{
    Task<List<Wishlist>> GetUserWishlistAsync(int userId);
    Task<Wishlist?> GetWishlistItemAsync(int userId, int productId);
    Task<bool> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> ClearWishlistAsync(int userId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
    Task<int> GetWishlistCountAsync(int userId);
    Task<List<Product>> GetWishlistProductsAsync(int userId, int page = 1, int pageSize = 12);
    Task<bool> MoveToCartAsync(int userId, int productId);
    Task<List<Wishlist>> GetWishlistWithProductInfoAsync(int userId);
    Task<bool> UpdateWishlistItemAsync(int wishlistId, DateTime? createdAt);
    Task<List<Wishlist>> GetRecentWishlistItemsAsync(int userId, int count = 5);
    Task<bool> RemoveExpiredWishlistItemsAsync(DateTime expirationDate);
    Task<List<Wishlist>> GetWishlistByProductIdAsync(int productId);
    Task<int> GetProductWishlistCountAsync(int productId);
}
