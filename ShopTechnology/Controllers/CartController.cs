using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(
            ICartService cartService,
            IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? string.Empty : string.Empty;
            var cart = await _cartService.GetCartViewModelAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? string.Empty : string.Empty;
            var result = await _cartService.AddToCartAsync(userId, productId, quantity);

            if (result)
            {
                TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng!";
            }
            else
            {
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ hàng.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var result = await _cartService.UpdateCartItemAsync(cartItemId, quantity);

            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var result = await _cartService.RemoveFromCartAsync(cartItemId);

            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart(int cartId)
        {
            var result = await _cartService.ClearCartAsync(cartId);

            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyPromoCode(string promoCode)
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? string.Empty : string.Empty;
            var result = await _cartService.ApplyPromoCodeAsync(userId, promoCode);

            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? string.Empty : string.Empty;
            var count = await _cartService.GetCartItemCountAsync(userId);

            return Json(new { count });
        }
    }
}
