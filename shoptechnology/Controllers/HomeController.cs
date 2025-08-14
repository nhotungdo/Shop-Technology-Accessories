using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;

namespace ShopTechnologyAccessories.Controllers;

public class HomeController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;

    public HomeController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 8)
    {
        var baseQuery = _db.Products
            .Include(p => p.ProductImages)
            .OrderByDescending(p => p.CreatedAt);
        var total = await baseQuery.CountAsync();
        var products = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var categories = await _db.Categories.ToListAsync();
        ViewBag.Categories = categories;
        var vm = new ProductListVm
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
        return View(vm);
    }
}


