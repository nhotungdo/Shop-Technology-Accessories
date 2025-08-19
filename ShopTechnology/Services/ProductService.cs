using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class ProductService : IProductService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public ProductService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Basic CRUD operations
    public async Task<List<ProductDTO>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<ProductDTO?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        return _mapper.Map<ProductDTO>(product);
    }

    public async Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDto)
    {
        var product = _mapper.Map<Product>(createProductDto);
        product.CreatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Add product images
        if (createProductDto.ImageUrls.Any())
        {
            var productImages = createProductDto.ImageUrls.Select((url, index) => new ProductImage
            {
                ProductId = product.ProductId,
                ImageUrl = url,
                IsMain = index == 0 // First image is main
            }).ToList();

            _context.ProductImages.AddRange(productImages);
            await _context.SaveChangesAsync();
        }

        return await GetProductByIdAsync(product.ProductId) ?? throw new InvalidOperationException("Failed to create product");
    }

    public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        _mapper.Map(updateProductDto, product);
        product.UpdatedAt = DateTime.UtcNow;

        // Update product images
        var existingImages = await _context.ProductImages.Where(pi => pi.ProductId == id).ToListAsync();
        _context.ProductImages.RemoveRange(existingImages);

        if (updateProductDto.ImageUrls.Any())
        {
            var productImages = updateProductDto.ImageUrls.Select((url, index) => new ProductImage
            {
                ProductId = product.ProductId,
                ImageUrl = url,
                IsMain = index == 0
            }).ToList();

            _context.ProductImages.AddRange(productImages);
        }

        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(id) ?? throw new InvalidOperationException("Failed to update product");
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }

    // Filtering and searching
    public async Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.CategoryId == categoryId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<List<ProductDTO>> SearchProductsAsync(string searchTerm)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.ProductName.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<List<ProductDTO>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    // Special queries
    public async Task<List<ProductDTO>> GetFeaturedProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<List<ProductDTO>> GetNewestProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .OrderByDescending(p => p.CreatedAt)
            .Take(12)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<List<ProductDTO>> GetTopSellingProductsAsync(int count)
    {
        // This would need to be implemented based on order history
        // For now, return newest products
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    public async Task<List<ProductDTO>> GetLowStockProductsAsync(int count)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.StockQuantity <= 10) // Low stock threshold
            .OrderBy(p => p.StockQuantity)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ProductDTO>>(products);
    }

    // Stock management
    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsProductInStockAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        return product?.StockQuantity > 0;
    }

    // Statistics
    public async Task<int> GetTotalProductsCountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<decimal> GetTotalProductsValueAsync()
    {
        return await _context.Products.SumAsync(p => p.Price * p.StockQuantity);
    }
}
