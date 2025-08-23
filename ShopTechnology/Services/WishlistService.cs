using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class WishlistService : IWishlistService
{
    private readonly ShopTechnologyAccessoriesContext _context;

    public WishlistService(ShopTechnologyAccessoriesContext context)
    {
        _context = context;
    }

    public async Task<List<Wishlist>> GetUserWishlistAsync(int userId)
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

    public async Task<Wishlist?> GetWishlistItemAsync(int userId, int productId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<bool> AddToWishlistAsync(int userId, int productId)
    {
        var existingItem = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existingItem != null)
            return false; // Đã có trong wishlist

        var wishlistItem = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.Now
        };

        _context.Wishlists.Add(wishlistItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        var wishlistItem = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (wishlistItem == null)
            return false;

        _context.Wishlists.Remove(wishlistItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearWishlistAsync(int userId)
    {
        var wishlistItems = await _context.Wishlists
            .Where(w => w.UserId == userId)
            .ToListAsync();

        if (!wishlistItems.Any())
            return false;

        _context.Wishlists.RemoveRange(wishlistItems);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<int> GetWishlistCountAsync(int userId)
    {
        return await _context.Wishlists
            .CountAsync(w => w.UserId == userId);
    }

    public async Task<List<Product>> GetWishlistProductsAsync(int userId, int page = 1, int pageSize = 12)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .ThenInclude(p => p.ProductImages)
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => w.Product)
            .ToListAsync();
    }

    public async Task<bool> MoveToCartAsync(int userId, int productId)
    {
        // Lấy cart của user
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Kiểm tra xem sản phẩm đã có trong cart chưa
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

        if (existingCartItem != null)
        {
            existingCartItem.Quantity++;
            existingCartItem.TotalPrice = existingCartItem.UnitPrice * existingCartItem.Quantity;
        }
        else
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                Quantity = 1,
                UnitPrice = product.Price,
                TotalPrice = product.Price,
                CreatedAt = DateTime.Now
            };
            _context.CartItems.Add(cartItem);
        }

        // Xóa khỏi wishlist
        await RemoveFromWishlistAsync(userId, productId);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Wishlist>> GetWishlistWithProductInfoAsync(int userId)
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
        var wishlistItem = await _context.Wishlists.FindAsync(wishlistId);
        if (wishlistItem == null) return false;

        if (createdAt.HasValue)
            wishlistItem.CreatedAt = createdAt.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Wishlist>> GetRecentWishlistItemsAsync(int userId, int count = 5)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> RemoveExpiredWishlistItemsAsync(DateTime expirationDate)
    {
        var expiredItems = await _context.Wishlists
            .Where(w => w.CreatedAt < expirationDate)
            .ToListAsync();

        if (!expiredItems.Any()) return false;

        _context.Wishlists.RemoveRange(expiredItems);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Wishlist>> GetWishlistByProductIdAsync(int productId)
    {
        return await _context.Wishlists
            .Include(w => w.User)
            .Where(w => w.ProductId == productId)
            .ToListAsync();
    }

    public async Task<int> GetProductWishlistCountAsync(int productId)
    {
        return await _context.Wishlists
            .CountAsync(w => w.ProductId == productId);
    }
}
