using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IProductService
{
    // Basic CRUD operations
    Task<List<ProductDTO>> GetAllProductsAsync();
    Task<ProductDTO?> GetProductByIdAsync(int id);
    Task<ProductDTO> CreateProductAsync(CreateProductDTO createProductDto);
    Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO updateProductDto);
    Task<bool> DeleteProductAsync(int id);
    
    // Filtering and searching
    Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId);
    Task<List<ProductDTO>> SearchProductsAsync(string searchTerm);
    Task<List<ProductDTO>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    
    // Special queries
    Task<List<ProductDTO>> GetFeaturedProductsAsync();
    Task<List<ProductDTO>> GetNewestProductsAsync();
    Task<List<ProductDTO>> GetTopSellingProductsAsync(int count);
    Task<List<ProductDTO>> GetLowStockProductsAsync(int count);
    
    // Stock management
    Task<bool> UpdateStockAsync(int productId, int quantity);
    Task<bool> IsProductInStockAsync(int productId);
    
    // Statistics
    Task<int> GetTotalProductsCountAsync();
    Task<decimal> GetTotalProductsValueAsync();
}
