using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly ICartService _cartService;
        private readonly IOrderFlowService _orderFlowService;

        public CartController(ShopTechnologyAccessoriesContext context, ICartService cartService, IOrderFlowService orderFlowService)
        {
            _context = context;
            _cartService = cartService;
            _orderFlowService = orderFlowService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Không thể xác định người dùng. Vui lòng đăng nhập lại." });
            }

            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 2: Thêm vào giỏ hàng ===");
                Console.WriteLine($"Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store");
                Console.WriteLine($"Parameters: UserId={userId}, ProductId={productId}, Quantity={quantity}");

                // Level 1: Thêm vào giỏ hàng - Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store
                var result = await _orderFlowService.AddToCartAsync(userId.Value, productId, quantity);

                if (result.Success)
                {
                    Console.WriteLine("Data Flow completed successfully");
                    return Json(new { success = true, message = result.Message });
                }

                Console.WriteLine($"Data Flow failed: {result.ErrorMessage}");
                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddToCart: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm vào giỏ hàng: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            var result = await _cartService.UpdateQuantityAsync(userId, cartItemId, quantity);

            if (result.Success)
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Json(new { 
                    success = true, 
                    message = "Số lượng đã được cập nhật.",
                    cartTotal = cart.TotalAmount,
                    itemCount = cart.Items.Count
                });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (result.Success)
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Json(new { 
                    success = true, 
                    message = "Sản phẩm đã được xóa khỏi giỏ hàng.",
                    cartTotal = cart.TotalAmount,
                    itemCount = cart.Items.Count
                });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);

            if (result.Success)
            {
                return Json(new { success = true, message = "Giỏ hàng đã được làm trống." });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyPromotion(string promotionCode)
        {
            var userId = GetUserId();
            var result = await _cartService.ApplyPromotionAsync(userId, promotionCode);

            if (result.Success)
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Json(new { 
                    success = true, 
                    message = "Mã khuyến mãi đã được áp dụng.",
                    cartTotal = cart.TotalAmount,
                    discountAmount = cart.DiscountAmount
                });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RemovePromotion()
        {
            var userId = GetUserId();
            var result = await _cartService.RemovePromotionAsync(userId);

            if (result.Success)
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Json(new { 
                    success = true, 
                    message = "Mã khuyến mãi đã được xóa.",
                    cartTotal = cart.TotalAmount,
                    discountAmount = cart.DiscountAmount
                });
            }

            return Json(new { success = false, message = result.Message });
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
    }
}
