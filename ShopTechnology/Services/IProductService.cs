using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductsAsync(ProductFilterViewModel filter, int page = 1, int pageSize = 12);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product?> GetProductBySlugAsync(string slug);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<Product>> GetNewProductsAsync(int count = 8);
        Task<IEnumerable<Product>> GetHotProductsAsync(int count = 8);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 4);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int count = 12);
        Task<bool> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId);
        Task<bool> AddProductImageAsync(ProductImage image);
        Task<bool> RemoveProductImageAsync(int imageId);
        Task<IEnumerable<ProductSpecification>> GetProductSpecificationsAsync(int productId);
        Task<bool> AddProductSpecificationAsync(ProductSpecification specification);
        Task<bool> UpdateProductSpecificationAsync(ProductSpecification specification);
        Task<bool> RemoveProductSpecificationAsync(int specificationId);
        Task<decimal> GetAverageRatingAsync(int productId);
        Task<int> GetReviewCountAsync(int productId);
        Task<bool> IncrementViewCountAsync(int productId);
        Task<bool> IncrementSoldCountAsync(int productId);
    }
}
