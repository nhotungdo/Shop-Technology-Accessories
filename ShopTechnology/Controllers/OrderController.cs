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
            var orders = await _orderService.GetOrderHistoryAsync(Guid.Parse(userIdStr));
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
            var (ok, orderId, message) = await _orderService.CreateOrderFromCartAsync(Guid.Parse(userIdStr), shippingAddress, paymentMethod);
            if (!ok)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction(nameof(Checkout));
            }
            TempData["SuccessMessage"] = "Đặt hàng thành công";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var order = await _orderService.GetOrderAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
