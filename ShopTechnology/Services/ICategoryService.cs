using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<List<Category>> GetCategoriesWithProductCountAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> CategoryExistsAsync(int id);
    Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);
}
