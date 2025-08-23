using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public ProductService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetFeaturedProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetNewProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetHotProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.IsHot)
                .OrderByDescending(p => p.SoldCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int page = 1, int pageSize = 12)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<List<string>> GetAllBrandsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand!)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm, int page = 1, int pageSize = 12)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Where(p => p.IsActive && 
                           (p.Name.Contains(searchTerm) || 
                            p.Description.Contains(searchTerm) || 
                            p.Brand.Contains(searchTerm) ||
                            p.SKU.Contains(searchTerm)))
                .OrderByDescending(p => p.ViewCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .CountAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new List<Product>();

            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && 
                           p.CategoryId == product.CategoryId && 
                           p.ProductId != productId)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateProductViewCountAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product != null && product.Reviews.Any())
            {
                product.AverageRating = product.Reviews.Average(r => r.Rating);
                product.ReviewCount = product.Reviews.Count;
                await _context.SaveChangesAsync();
            }
        }
    }
}
