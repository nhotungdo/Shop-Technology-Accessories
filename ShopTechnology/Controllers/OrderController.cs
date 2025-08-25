using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IOrderService _orderService;

        public OrderController(ShopTechnologyAccessoriesContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
                    .Where(o => o.UserId == int.Parse(userId))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var orderViewModels = orders.Select(order => new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedAt,
                    Status = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel
                    {
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ProductImage = od.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? od.Product?.MainImage ?? "/img/best-tech-accessories.png"
                    }).ToList()
                }).ToList();

                return View(orderViewModels);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading order history: {ex.Message}");
                return View(new List<OrderViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
                    .Include(o => o.OrderHistories)
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == int.Parse(userId));

                if (order == null)
                {
                    return NotFound();
                }

                var orderViewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedAt,
                    Status = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    CustomerPhone = order.CustomerPhone,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel
                    {
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ProductImage = od.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? od.Product?.MainImage ?? "/img/best-tech-accessories.png"
                    }).ToList(),
                    OrderHistories = order.OrderHistories?.Select(oh => new OrderHistoryViewModel
                    {
                        Status = oh.Status,
                        CreatedAt = oh.CreatedAt,
                        Note = oh.Notes ?? string.Empty
                    }).OrderByDescending(oh => oh.CreatedAt).ToList() ?? new List<OrderHistoryViewModel>()
                };

                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order details: {ex.Message}");
                return View("Error");
            }
        }
    }
}
