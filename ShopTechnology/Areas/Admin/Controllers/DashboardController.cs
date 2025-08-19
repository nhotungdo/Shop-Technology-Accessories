using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;

    public DashboardController(
        IProductService productService,
        IOrderService orderService,
        IUserService userService,
        ICategoryService categoryService)
    {
        _productService = productService;
        _orderService = orderService;
        _userService = userService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboardViewModel = new DashboardViewModel
            {
                TotalProducts = await _productService.GetTotalProductsCountAsync(),
                TotalOrders = await _orderService.GetTotalOrdersCountAsync(),
                TotalUsers = await _userService.GetTotalUsersCountAsync(),
                TotalCategories = await _categoryService.GetTotalCategoriesCountAsync(),
                
                // Recent orders
                RecentOrders = await _orderService.GetRecentOrdersAsync(5),
                
                // Top selling products
                TopSellingProducts = await _productService.GetTopSellingProductsAsync(5),
                
                // Low stock products
                LowStockProducts = await _productService.GetLowStockProductsAsync(5)
            };

            return View(dashboardViewModel);
        }
        catch (Exception ex)
        {
            // Log the exception
            ModelState.AddModelError("", "An error occurred while loading dashboard data.");
            return View(new DashboardViewModel());
        }
    }

    public async Task<IActionResult> Home()
    {
        // Redirect to Index action
        return RedirectToAction("Index");
    }

    // Fallback action for any other routes
    public async Task<IActionResult> Default()
    {
        return RedirectToAction("Index");
    }
}

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    public List<OrderDTO> RecentOrders { get; set; } = new();
    public List<ProductDTO> TopSellingProducts { get; set; } = new();
    public List<ProductDTO> LowStockProducts { get; set; } = new();
}
