using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;
using System.Security.Claims;

namespace ShopTechnologyAccessories.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;
    public OrdersController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var userId = GetUserId();
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
        if (order == null) return NotFound();
        return View(order);
    }
}


