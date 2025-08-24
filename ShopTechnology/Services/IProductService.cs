using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetFeaturedProductsAsync(int count);
        Task<List<Product>> GetNewProductsAsync(int count);
        Task<List<Product>> GetHotProductsAsync(int count);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int page = 1, int pageSize = 12);
        Task<Product?> GetProductBySlugAsync(string slug);
        Task<List<string>> GetAllBrandsAsync();
        Task<List<Product>> SearchProductsAsync(string searchTerm, int page = 1, int pageSize = 12);
        Task<int> GetTotalProductsCountAsync();
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);
        Task UpdateProductViewCountAsync(int productId);
        Task UpdateProductRatingAsync(int productId);
        Task<List<Product>> GetProductsAsync(int page = 1, int pageSize = 12);
        Task<PaginatedResult<Product>> GetProductsAsync(int? categoryId, string? searchTerm, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<List<Product>> GetOutOfStockProductsAsync();
    }
}
