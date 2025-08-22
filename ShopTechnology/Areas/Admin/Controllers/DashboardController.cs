using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IUserService _userService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ShopTechnologyAccessoriesContext context,
        IOrderService orderService,
        IProductService productService,
        IUserService userService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _orderService = orderService;
        _productService = productService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            // Statistics
            var totalRevenue = await _orderService.GetTotalRevenueAsync();
            var monthlyRevenue = await _orderService.GetTotalRevenueAsync(thisMonth, today);
            var lastMonthRevenue = await _orderService.GetTotalRevenueAsync(lastMonth, thisMonth.AddDays(-1));

            var totalOrders = await _orderService.GetOrderCountAsync();
            var monthlyOrders = await _orderService.GetOrderCountAsync(thisMonth, today);
            var lastMonthOrders = await _orderService.GetOrderCountAsync(lastMonth, thisMonth.AddDays(-1));

            var totalProducts = await _context.Products.CountAsync();
            var lowStockProducts = await _productService.GetLowStockProductsAsync(10);
            var outOfStockProducts = await _productService.GetOutOfStockProductsAsync();

            var totalUsers = await _context.Users.CountAsync();
            var newUsersThisMonth = await _context.Users
                .Where(u => u.CreatedAt >= thisMonth)
                .CountAsync();

            // Recent orders
            var recentOrders = await _orderService.GetRecentOrdersAsync(5);

            // Top selling products
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.ProductName,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                RevenueGrowth = lastMonthRevenue > 0 ? ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0,
                
                TotalOrders = totalOrders,
                MonthlyOrders = monthlyOrders,
                OrderGrowth = lastMonthOrders > 0 ? ((monthlyOrders - lastMonthOrders) / lastMonthOrders) * 100 : 0,
                
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts.Count,
                OutOfStockProducts = outOfStockProducts.Count,
                
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                
                RecentOrders = recentOrders,
                TopSellingProducts = topProducts.Select(p => new TopProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    TotalSold = p.TotalSold,
                    TotalRevenue = p.TotalRevenue
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetChartData()
    {
        try
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            var dailyRevenue = new List<ChartDataViewModel>();
            var dailyOrders = new List<ChartDataViewModel>();

            foreach (var date in last7Days)
            {
                var nextDate = date.AddDays(1);
                var revenue = await _orderService.GetTotalRevenueAsync(date, nextDate);
                var orders = await _orderService.GetOrderCountAsync(date, nextDate);

                dailyRevenue.Add(new ChartDataViewModel
                {
                    Label = date.ToString("dd/MM"),
                    Value = revenue
                });

                dailyOrders.Add(new ChartDataViewModel
                {
                    Label = date.ToString("dd/MM"),
                    Value = orders
                });
            }

            return Json(new
            {
                revenue = dailyRevenue,
                orders = dailyOrders
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart data");
            return Json(new { error = "Failed to load chart data" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderStatusData()
    {
        try
        {
            var statuses = new[] { "Pending", "Paid", "Shipped", "Completed", "Cancelled" };
            var statusData = new List<ChartDataViewModel>();

            foreach (var status in statuses)
            {
                var count = await _orderService.GetOrderCountAsync();
                statusData.Add(new ChartDataViewModel
                {
                    Label = status,
                    Value = count
                });
            }

            return Json(statusData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order status data");
            return Json(new { error = "Failed to load status data" });
        }
    }
}

public class AdminDashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    
    public int TotalOrders { get; set; }
    public int MonthlyOrders { get; set; }
    public decimal OrderGrowth { get; set; }
    
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    
    public List<Order> RecentOrders { get; set; } = new();
    public List<TopProductViewModel> TopSellingProducts { get; set; } = new();
}

public class TopProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ChartDataViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
