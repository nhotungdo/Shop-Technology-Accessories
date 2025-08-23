using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetFeaturedCategoriesAsync(int count);
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<List<Category>> GetSubCategoriesAsync(int parentCategoryId);
    }
}
