using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(string status, string searchTerm, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                orders = orders.Where(o => o.Status == status).ToList();
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => o.UserFullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         o.UserEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         o.OrderId.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Date >= startDate.Value.Date).ToList();
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Date <= endDate.Value.Date).ToList();
            }

            // Sort by order date descending
            orders = orders.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.Status = status;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(orders);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading orders.");
            return View(new List<OrderDTO>());
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
            ModelState.AddModelError("", "An error occurred while loading the order.");
            return View(new OrderDTO());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusDTO updateOrderStatusDto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid status update request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto.Status);
            TempData["Success"] = "Order status updated successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while updating the order status.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _orderService.CancelOrderAsync(id);
            TempData["Success"] = "Order cancelled successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while cancelling the order.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            
            // Create CSV content
            var csvContent = "Order ID,User Name,User Email,Order Date,Total Amount,Status,Shipping Address\n";
            
            foreach (var order in orders)
            {
                csvContent += $"{order.OrderId},{order.UserFullName},{order.UserEmail},{order.OrderDate:yyyy-MM-dd HH:mm:ss},{order.TotalAmount},{order.Status},{order.ShippingAddress}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            var fileName = $"orders_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while exporting orders.";
            return RedirectToAction(nameof(Index));
        }
    }
}
