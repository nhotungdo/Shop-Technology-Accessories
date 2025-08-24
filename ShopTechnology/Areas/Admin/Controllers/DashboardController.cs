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

            // Get recent orders with user information
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    UserFullName = o.User.FullName,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus,
                    StatusDisplay = GetStatusDisplay(o.OrderStatus)
                })
                .ToListAsync();

            // Get low stock products
            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= 10)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => new LowStockProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    StockQuantity = p.StockQuantity,
                    Price = p.Price
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts
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


