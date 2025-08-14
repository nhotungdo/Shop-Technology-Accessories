using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;

namespace ShopTechnologyAccessories.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;
    public DashboardController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var revenueToday = await _db.Orders
            .Where(o => o.OrderDate >= today)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        var ordersToday = await _db.Orders.CountAsync(o => o.OrderDate >= today);
        var totalOrders = await _db.Orders.CountAsync();
        var totalProducts = await _db.Products.CountAsync();
        ViewBag.RevenueToday = revenueToday;
        ViewBag.OrdersToday = ordersToday;
        ViewBag.TotalOrders = totalOrders;
        ViewBag.TotalProducts = totalProducts;

        // Recent 5 orders
        var recentOrders = await _db.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Include(o => o.User)
            .ToListAsync();
        ViewBag.RecentOrders = recentOrders;

        // Top 5 products by quantity in order details
        var topProducts = await _db.OrderDetails
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .Join(_db.Products,
                g => g.ProductId,
                p => p.ProductId,
                (g, p) => new { p.ProductId, p.ProductName, g.Quantity })
            .ToListAsync();
        ViewBag.TopProducts = topProducts;

        // Sales/Orders last 6 months
        var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-5);
        var ordersWindow = await _db.Orders
            .Where(o => o.OrderDate >= startMonth)
            .ToListAsync();

        var labels = new List<string>();
        var salesSeries = new List<decimal>();
        var ordersSeries = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            var from = startMonth.AddMonths(i);
            var to = from.AddMonths(1);
            labels.Add(from.ToString("MMMM"));
            var monthOrders = ordersWindow.Where(o => o.OrderDate >= from && o.OrderDate < to).ToList();
            salesSeries.Add(monthOrders.Sum(o => o.TotalAmount));
            ordersSeries.Add(monthOrders.Count);
        }
        ViewBag.ChartMonths = labels;
        ViewBag.ChartSales = salesSeries;
        ViewBag.ChartOrders = ordersSeries;

        return View();
    }
}


