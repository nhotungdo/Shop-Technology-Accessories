using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        ShopTechnologyAccessoriesContext context,
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate, int page = 1)
    {
        try
        {
            const int pageSize = 20;
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders list");
            return View("Error");
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order details for ID: {OrderId}", id);
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid orderId, string newStatus)
    {
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            if (result)
            {
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng thành công: {newStatus}";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for ID: {OrderId}", orderId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(orderId);
            if (result)
            {
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order for ID: {OrderId}", orderId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        try
        {
            var pendingOrders = await _orderService.GetOrdersByStatusAsync("Pending");
            return View("Index", pendingOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending orders");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Completed()
    {
        try
        {
            var completedOrders = await _orderService.GetOrdersByStatusAsync("Completed");
            return View("Index", completedOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading completed orders");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Cancelled()
    {
        try
        {
            var cancelledOrders = await _orderService.GetOrdersByStatusAsync("Cancelled");
            return View("Index", cancelledOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cancelled orders");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Reports()
    {
        try
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var totalRevenue = await _orderService.GetTotalRevenueAsync();
            var monthlyRevenue = await _orderService.GetTotalRevenueAsync(thisMonth, today);
            var lastMonthRevenue = await _orderService.GetTotalRevenueAsync(lastMonth, thisMonth.AddDays(-1));

            var totalOrders = await _orderService.GetOrderCountAsync();
            var monthlyOrders = await _orderService.GetOrderCountAsync(thisMonth, today);
            var lastMonthOrders = await _orderService.GetOrderCountAsync(lastMonth, thisMonth.AddDays(-1));

            var pendingOrders = await _orderService.GetOrdersByStatusAsync("Pending");
            var completedOrders = await _orderService.GetOrdersByStatusAsync("Completed");
            var cancelledOrders = await _orderService.GetOrdersByStatusAsync("Cancelled");

            var viewModel = new OrderReportViewModel
            {
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                RevenueGrowth = lastMonthRevenue > 0 ? ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0,
                
                TotalOrders = totalOrders,
                MonthlyOrders = monthlyOrders,
                OrderGrowth = lastMonthOrders > 0 ? ((monthlyOrders - lastMonthOrders) / lastMonthOrders) * 100 : 0,
                
                PendingOrders = pendingOrders.Count,
                CompletedOrders = completedOrders.Count,
                CancelledOrders = cancelledOrders.Count
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order reports");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportOrders(string? status, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Generate CSV content
            var csvContent = "Order ID,User,Order Date,Status,Total Amount,Payment Method,Shipping Address\n";
            
            foreach (var order in orders)
            {
                csvContent += $"{order.OrderId},{order.User?.FullName},{order.OrderDate:yyyy-MM-dd HH:mm},{order.Status},{order.TotalAmount:N0},{order.Payment?.Method},{order.ShippingAddress}\n";
            }

            var fileName = $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting orders");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất dữ liệu";
            return RedirectToAction(nameof(Index));
        }
    }
}

public class OrderReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    
    public int TotalOrders { get; set; }
    public int MonthlyOrders { get; set; }
    public decimal OrderGrowth { get; set; }
    
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
}
