using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> History()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var orders = await _orderService.GetOrdersByUserIdAsync(Guid.Parse(userIdStr));
            return View(orders);
        }

        public IActionResult Checkout()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            // TODO: Implement CreateOrderFromCartAsync or use CreateOrderAsync with proper model
            TempData["ErrorMessage"] = "Order creation not implemented yet";
            return RedirectToAction(nameof(Checkout));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
