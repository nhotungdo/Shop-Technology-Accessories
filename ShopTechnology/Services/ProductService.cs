using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(ProductFilterViewModel filter, int page = 1, int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                        p.Description.Contains(filter.SearchTerm) ||
                                        (p.Brand ?? "").Contains(filter.SearchTerm));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice);
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                query = query.Where(p => p.Brand == filter.Brand || (p.Brand == null && filter.Brand == null));
            }

            if (filter.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == filter.IsFeatured);
            }

            if (filter.IsNew.HasValue)
            {
                query = query.Where(p => p.IsNew == filter.IsNew);
            }

            if (filter.IsHot.HasValue)
            {
                query = query.Where(p => p.IsHot == filter.IsHot);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(img => img.DisplayOrder))
                .Include(p => p.Reviews.OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.User)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.ReviewImages)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(img => img.DisplayOrder))
                .Include(p => p.Reviews.OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.User)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.ReviewImages)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetNewProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive && p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetHotProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive && p.IsHot)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 4)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return new List<Product>();

            return await _context.Products
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive &&
                           p.CategoryId == product.CategoryId &&
                           p.ProductId != productId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 12)
        {
            return await _context.Products
                .Include(p => p.ProductImages.Where(img => img.IsMain))
                .Include(p => p.Reviews)
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                product.UpdatedAt = DateTime.UtcNow;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                product.StockQuantity = quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId)
        {
            return await _context.ProductImages
                .Where(img => img.ProductId == productId)
                .OrderBy(img => img.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> AddProductImageAsync(ProductImage image)
        {
            try
            {
                image.CreatedAt = DateTime.UtcNow;
                _context.ProductImages.Add(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveProductImageAsync(int imageId)
        {
            try
            {
                var image = await _context.ProductImages.FindAsync(imageId);
                if (image == null) return false;

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }



        public async Task<decimal> GetAverageRatingAsync(int productId)
        {
            var averageRating = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => (decimal)r.Rating);

            return Math.Round(averageRating, 1);
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId);
        }

        public async Task<bool> IncrementViewCountAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                // Note: You might want to add a ViewCount property to Product model
                // product.ViewCount++;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IncrementSoldCountAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                // Note: You might want to add a SoldCount property to Product model
                // product.SoldCount++;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
