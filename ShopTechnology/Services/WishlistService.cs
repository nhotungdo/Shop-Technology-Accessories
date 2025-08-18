using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public WishlistService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Guid userId, int productId)
        {
            var exists = await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            if (exists) return true;
            _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(Guid userId, int productId)
        {
            var item = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            if (item == null) return false;
            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductViewModel>> GetAllAsync(Guid userId)
        {
            var products = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.Product)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty,
                ImageUrls = p.ProductImages?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? string.Empty,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }
    }
}
