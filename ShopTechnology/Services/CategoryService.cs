using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ChildCategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ChildCategories.Where(child => child.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ChildCategories.Where(child => child.IsActive))
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedAt = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                category.UpdatedAt = DateTime.UtcNow;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return false;

                // Check if category has products
                var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts) return false;

                // Check if category has children
                var hasChildren = await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
                if (hasChildren) return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Category>> GetCategoryTreeAsync()
        {
            return await _context.Categories
                .Include(c => c.ChildCategories.Where(child => child.IsActive))
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}
