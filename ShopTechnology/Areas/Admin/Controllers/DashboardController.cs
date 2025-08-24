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
            // Get basic statistics
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            // Get revenue statistics
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-7);
            var thisMonth = today.AddMonths(-1);

            var todayRevenue = await _orderService.GetTotalRevenueAsync(today, today.AddDays(1));
            var weekRevenue = await _orderService.GetTotalRevenueAsync(thisWeek, today.AddDays(1));
            var monthRevenue = await _orderService.GetTotalRevenueAsync(thisMonth, today.AddDays(1));

            // Get order statistics
            var todayOrders = await _orderService.GetOrderCountAsync(today, today.AddDays(1));
            var weekOrders = await _orderService.GetOrderCountAsync(thisWeek, today.AddDays(1));
            var monthOrders = await _orderService.GetOrderCountAsync(thisMonth, today.AddDays(1));

            // Get new users statistics
            var todayUsers = await _context.Users.CountAsync(u => u.CreatedAt >= today);
            var weekUsers = await _context.Users.CountAsync(u => u.CreatedAt >= thisWeek);
            var monthUsers = await _context.Users.CountAsync(u => u.CreatedAt >= thisMonth);

            // Get recent orders with user information
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    UserFullName = o.User.FullName,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus,
                    StatusDisplay = GetStatusDisplay(o.OrderStatus),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            // Get low stock products
            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= 10)
                .OrderBy(p => p.StockQuantity)
                .Take(10)
                .Select(p => new LowStockProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    StockQuantity = p.StockQuantity,
                    Price = p.Price
                })
                .ToListAsync();

            // Get top selling products
            var topSellingProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .Join(_context.Products, x => x.ProductId, p => p.ProductId, (x, p) => new TopSellingProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    TotalSold = x.TotalSold,
                    Price = p.Price
                })
                .ToListAsync();

            // Get pending orders count
            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");

            var viewModel = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                TodayRevenue = todayRevenue,
                WeekRevenue = weekRevenue,
                MonthRevenue = monthRevenue,
                TodayOrders = todayOrders,
                WeekOrders = weekOrders,
                MonthOrders = monthOrders,
                TodayUsers = todayUsers,
                WeekUsers = weekUsers,
                MonthUsers = monthUsers,
                PendingOrders = pendingOrders,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts,
                TopSellingProducts = topSellingProducts
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View("Error");
        }
    }

    private string GetStatusDisplay(string status)
    {
        return status switch
        {
            "Pending" => "Chờ xử lý",
            "Paid" => "Đã thanh toán",
            "Shipped" => "Đã giao hàng",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            _ => status
        };
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


