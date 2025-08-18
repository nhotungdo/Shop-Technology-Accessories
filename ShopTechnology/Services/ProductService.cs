using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using AutoMapper;

namespace ShopTechnology.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IMapper _mapper;

        public ProductService(ShopTechnologyAccessoriesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<ProductViewModel?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName ?? "",
                ImageUrls = product.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = product.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<List<ProductViewModel>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<List<ProductViewModel>> SearchProductsAsync(string searchTerm)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.ProductName.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<List<ProductViewModel>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<List<ProductViewModel>> GetFeaturedProductsAsync()
        {
            // Lấy 8 sản phẩm mới nhất làm sản phẩm nổi bật
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<List<ProductViewModel>> GetNewestProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            return products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? "",
                ImageUrls = p.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                MainImageUrl = p.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }

        public async Task<bool> CreateProductAsync(ProductViewModel product)
        {
            try
            {
                var newProduct = new Product
                {
                    CategoryId = product.CategoryId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    CreatedAt = DateTime.Now
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                // Thêm ảnh sản phẩm nếu có
                if (product.ImageUrls.Any())
                {
                    var productImages = product.ImageUrls.Select((url, index) => new ProductImage
                    {
                        ProductId = newProduct.ProductId,
                        ImageUrl = url,
                        IsMain = index == 0 // Ảnh đầu tiên là ảnh chính
                    }).ToList();

                    _context.ProductImages.AddRange(productImages);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductViewModel product)
        {
            try
            {
                var existingProduct = await _context.Products.FindAsync(product.ProductId);
                if (existingProduct == null) return false;

                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.UpdatedAt = DateTime.Now;

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

                _context.Products.Remove(product);
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
                product.UpdatedAt = DateTime.Now;
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
