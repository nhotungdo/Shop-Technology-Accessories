using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public class ProductService : IProductService
{
    private readonly ShopTechnologyAccessoriesContext _context;

    public ProductService(ShopTechnologyAccessoriesContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<PagedResult<Product>> GetProductsAsync(
        int? categoryId = null,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 12)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.Reviews)
            .AsQueryable();

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.ProductName.Contains(searchTerm) ||
                (p.Description != null && p.Description.Contains(searchTerm)));
        }

        // Filter by price range
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Sort products
        query = sortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.ProductName),
            "name_desc" => query.OrderByDescending(p => p.ProductName),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Rating),
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
            PageSize = pageSize
        };
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.StockQuantity > 0)
            .OrderByDescending(p => p.Rating)
            .ThenByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> GetLatestProductsAsync(int count = 6)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return new List<Product>();

        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId)
            .Where(p => p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int count = 12)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.CategoryId == categoryId && p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, int count = 20)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.ProductName.Contains(searchTerm) ||
                       (p.Description != null && p.Description.Contains(searchTerm)))
            .Where(p => p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        if (product.StockQuantity < quantity)
            return false;

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= threshold && p.StockQuantity > 0)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity == 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
