using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public CategoryService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => true /* c.IsActive - removed because column doesn't exist */)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetFeaturedCategoriesAsync(int count)
        {
            return await _context.Categories
                .Where(c => true /* c.IsActive - removed because column doesn't exist */ && c.IsFeatured)
                .OrderBy(c => c.DisplayOrder)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Slug == slug && true /* c.IsActive - removed because column doesn't exist */);
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId && true /* c.IsActive - removed because column doesn't exist */)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }
    }
}
