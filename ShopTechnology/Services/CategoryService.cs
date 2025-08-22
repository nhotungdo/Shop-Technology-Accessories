using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

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
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == name.ToLower());
    }

    public async Task<List<Category>> GetCategoriesWithProductCountAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .Select(c => new Category
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                Products = c.Products.Where(p => p.StockQuantity > 0).ToList()
            })
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
        if (existingCategory == null)
            return false;

        existingCategory.CategoryName = category.CategoryName;
        existingCategory.Description = category.Description;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        // Check if category has products
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            return false; // Cannot delete category with products

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CategoryExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.CategoryId == id);
    }

    public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Categories.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CategoryId != excludeId.Value);
        }

        return await query.AnyAsync(c => c.CategoryName.ToLower() == name.ToLower());
    }
}
