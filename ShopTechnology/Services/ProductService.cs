using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

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
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetNewProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetHotProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsHot)
                .OrderByDescending(p => p.SoldCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int page = 1, int pageSize = 12)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
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
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<List<string>> GetAllBrandsAsync()
        {
            return await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Brand))
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
                .Where(p => (p.Name.Contains(searchTerm) ||
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
                .CountAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new List<Product>();

            return await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId &&
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
                product.AverageRating = (decimal)product.Reviews.Average(r => r.Rating);
                product.ReviewCount = product.Reviews.Count;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetProductsAsync(int page = 1, int pageSize = 12)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<PaginatedResult<Product>> GetProductsAsync(int? categoryId, string? searchTerm, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) ||
                                       p.Description.Contains(searchTerm) ||
                                       (p.Brand != null && p.Brand.Contains(searchTerm)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Get total count before applying pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.StockQuantity <= threshold && p.StockQuantity > 0)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<List<Product>> GetOutOfStockProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.StockQuantity == 0)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
