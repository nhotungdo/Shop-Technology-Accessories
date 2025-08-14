using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;

namespace ShopTechnologyAccessories.Controllers;

public class ProductsController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;

    public ProductsController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 8)
    {
        var query = _db.Products.Include(p => p.ProductImages).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.ProductName.Contains(q));
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        ViewBag.Categories = await _db.Categories.ToListAsync();
        var total = await query.CountAsync();
        var products = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var vm = new ProductListVm
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
            Q = q,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductImages)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();
        return View(product);
    }
}


