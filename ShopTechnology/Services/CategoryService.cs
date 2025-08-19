using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class CategoryService : ICategoryService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public CategoryService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return _mapper.Map<List<CategoryDTO>>(categories);
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        return _mapper.Map<CategoryDTO>(category);
    }

    public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO createCategoryDto)
    {
        // Check if category name already exists
        if (await IsCategoryNameExistsAsync(createCategoryDto.CategoryName))
        {
            throw new InvalidOperationException("Category name already exists");
        }

        var category = _mapper.Map<Category>(createCategoryDto);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(category.CategoryId) ?? throw new InvalidOperationException("Failed to create category");
    }

    public async Task<CategoryDTO> UpdateCategoryAsync(int categoryId, UpdateCategoryDTO updateCategoryDto)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        // Check if new name conflicts with existing category
        if (category.CategoryName != updateCategoryDto.CategoryName && 
            await IsCategoryNameExistsAsync(updateCategoryDto.CategoryName))
        {
            throw new InvalidOperationException("Category name already exists");
        }

        _mapper.Map(updateCategoryDto, category);

        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(categoryId) ?? throw new InvalidOperationException("Failed to update category");
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return false;
        }

        // Check if category has products
        if (category.Products.Any())
        {
            throw new InvalidOperationException("Cannot delete category that has products");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsCategoryNameExistsAsync(string categoryName)
    {
        return await _context.Categories.AnyAsync(c => c.CategoryName == categoryName);
    }

    public async Task<int> GetTotalCategoriesCountAsync()
    {
        return await _context.Categories.CountAsync();
    }

    public async Task<List<CategoryDTO>> GetCategoriesWithProductCountAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        var categoryDtos = _mapper.Map<List<CategoryDTO>>(categories);

        // Set product count for each category
        foreach (var categoryDto in categoryDtos)
        {
            var category = categories.First(c => c.CategoryId == categoryDto.CategoryId);
            categoryDto.ProductCount = category.Products.Count;
        }

        return categoryDtos;
    }
}
