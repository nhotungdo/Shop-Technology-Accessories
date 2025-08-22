using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int id);
    Task<PagedResult<Product>> GetProductsAsync(
        int? categoryId = null, 
        string? searchTerm = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        string? sortBy = null, 
        int page = 1, 
        int pageSize = 12);
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
    Task<List<Product>> GetLatestProductsAsync(int count = 6);
    Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int count = 12);
    Task<List<Product>> SearchProductsAsync(string searchTerm, int count = 20);
    Task<bool> UpdateStockAsync(int productId, int quantity);
    Task<List<Product>> GetLowStockProductsAsync(int threshold = 10);
    Task<List<Product>> GetOutOfStockProductsAsync();
}
