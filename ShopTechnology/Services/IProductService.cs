using ShopTechnology.Models;

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
    }
}
