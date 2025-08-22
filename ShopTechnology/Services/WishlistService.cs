using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class WishlistService : IWishlistService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(ShopTechnologyAccessoriesContext context, ILogger<WishlistService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Wishlist>> GetWishlistByUserIdAsync(Guid userId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.ProductImages)
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<Wishlist?> GetWishlistItemAsync(Guid userId, int productId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<bool> AddToWishlistAsync(Guid userId, int productId)
    {
        try
        {
            // Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            // Check if already in wishlist
            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existingItem != null)
            {
                return false; // Already in wishlist
            }

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} added to wishlist for user {UserId}", productId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to wishlist for user {UserId}", productId, userId);
            return false;
        }
    }

    public async Task<bool> RemoveFromWishlistAsync(Guid userId, int productId)
    {
        try
        {
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
            {
                return false;
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} removed from wishlist for user {UserId}", productId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from wishlist for user {UserId}", productId, userId);
            return false;
        }
    }

    public async Task<bool> ClearWishlistAsync(Guid userId)
    {
        try
        {
            var wishlistItems = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (!wishlistItems.Any())
            {
                return true; // Already empty
            }

            _context.Wishlists.RemoveRange(wishlistItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wishlist cleared for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, int productId)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<int> GetWishlistCountAsync(Guid userId)
    {
        return await _context.Wishlists
            .CountAsync(w => w.UserId == userId);
    }

    public async Task<List<Product>> GetWishlistProductsAsync(Guid userId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.ProductImages)
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .Select(w => w.Product)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MoveToCartAsync(Guid userId, int productId)
    {
        try
        {
            // Check if product is in wishlist
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlistItem == null)
            {
                return false;
            }

            // Check if product is already in cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create new cart
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingCartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // Product already in cart, just remove from wishlist
                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                return true;
            }

            // Add to cart
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                Quantity = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);

            // Remove from wishlist
            _context.Wishlists.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {ProductId} moved from wishlist to cart for user {UserId}", productId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving product {ProductId} from wishlist to cart for user {UserId}", productId, userId);
            return false;
        }
    }

    public async Task<List<Wishlist>> GetWishlistWithProductInfoAsync(Guid userId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.ProductImages)
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateWishlistItemAsync(int wishlistId, DateTime? createdAt)
    {
        try
        {
            var wishlistItem = await _context.Wishlists.FindAsync(wishlistId);
            if (wishlistItem == null)
            {
                return false;
            }

            if (createdAt.HasValue)
            {
                wishlistItem.CreatedAt = createdAt.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wishlist item {WishlistId}", wishlistId);
            return false;
        }
    }

    public async Task<List<Wishlist>> GetRecentWishlistItemsAsync(Guid userId, int count = 5)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.ProductImages)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> RemoveExpiredWishlistItemsAsync(DateTime expirationDate)
    {
        try
        {
            var expiredItems = await _context.Wishlists
                .Where(w => w.CreatedAt < expirationDate)
                .ToListAsync();

            if (!expiredItems.Any())
            {
                return true;
            }

            _context.Wishlists.RemoveRange(expiredItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed {Count} expired wishlist items", expiredItems.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing expired wishlist items");
            return false;
        }
    }
}
