using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IProductService
    {
        Task<List<ProductViewModel>> GetAllProductsAsync();
        Task<ProductViewModel?> GetProductByIdAsync(int id);
        Task<List<ProductViewModel>> GetProductsByCategoryAsync(int categoryId);
        Task<List<ProductViewModel>> SearchProductsAsync(string searchTerm);
        Task<List<ProductViewModel>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<List<ProductViewModel>> GetFeaturedProductsAsync();
        Task<List<ProductViewModel>> GetNewestProductsAsync();
        Task<bool> CreateProductAsync(ProductViewModel product);
        Task<bool> UpdateProductAsync(ProductViewModel product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
