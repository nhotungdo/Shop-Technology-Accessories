using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface ICategoryService
{
    Task<List<CategoryDTO>> GetAllCategoriesAsync();
    Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId);
    Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO createCategoryDto);
    Task<CategoryDTO> UpdateCategoryAsync(int categoryId, UpdateCategoryDTO updateCategoryDto);
    Task<bool> DeleteCategoryAsync(int categoryId);
    Task<bool> IsCategoryNameExistsAsync(string categoryName);
    Task<int> GetTotalCategoriesCountAsync();
    Task<List<CategoryDTO>> GetCategoriesWithProductCountAsync();
}
