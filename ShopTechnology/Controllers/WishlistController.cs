using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICartService _cartService;

        public WishlistController(IWishlistService wishlistService, ICartService cartService)
        {
            _wishlistService = wishlistService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItems = await _wishlistService.GetUserWishlistAsync(userId.Value);

            // Lấy thông tin cart để kiểm tra sản phẩm có trong cart không
            var cart = await _cartService.GetCartAsync(userId);
            var cartProductIds = cart.Items.Select(i => i.ProductId).ToHashSet();

            // Chuyển đổi từ Wishlist sang ProductViewModel
            var productViewModels = wishlistItems.Select(w => new ProductViewModel
            {
                ProductId = w.Product.ProductId,
                ProductName = w.Product.Name,
                Description = w.Product.Description ?? string.Empty,
                Price = w.Product.Price,
                OriginalPrice = w.Product.OriginalPrice ?? 0,
                StockQuantity = w.Product.StockQuantity,
                SKU = w.Product.SKU ?? string.Empty,
                Slug = w.Product.Slug ?? string.Empty,
                Brand = w.Product.Brand ?? string.Empty,
                CategoryName = w.Product.Category?.Name ?? string.Empty,
                CategoryId = w.Product.CategoryId,
                AverageRating = w.Product.AverageRating ?? 0,
                ReviewCount = 0, // Có thể tính từ Reviews nếu cần
                MainImageUrl = w.Product.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ??
                              w.Product.ProductImages?.FirstOrDefault()?.ImageUrl ??
                              "/images/no-image.png",
                ImageUrls = w.Product.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>(),
                IsInWishlist = true,
                IsInCart = cartProductIds.Contains(w.Product.ProductId),
                CartQuantity = cart.Items.FirstOrDefault(i => i.ProductId == w.Product.ProductId)?.Quantity ?? 0,
                CreatedAt = w.Product.CreatedAt
            }).ToList();

            return View(productViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích." });
            }

            var result = await _wishlistService.AddToWishlistAsync(userId.Value, productId);
            if (!result)
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích." });
            }

            return Json(new { success = true, message = "Sản phẩm đã được thêm vào danh sách yêu thích." });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var result = await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);
            if (!result)
            {
                return Json(new { success = false, message = "Sản phẩm không có trong danh sách yêu thích." });
            }

            return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi danh sách yêu thích." });
        }

        [HttpPost]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var result = await _wishlistService.ClearWishlistAsync(userId.Value);
            if (!result)
            {
                return Json(new { success = false, message = "Danh sách yêu thích đã trống." });
            }

            return Json(new { success = true, message = "Danh sách yêu thích đã được làm trống." });
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var result = await _wishlistService.MoveToCartAsync(userId.Value, productId);
            if (!result)
            {
                return Json(new { success = false, message = "Không thể chuyển sản phẩm vào giỏ hàng." });
            }

            return Json(new { success = true, message = "Sản phẩm đã được chuyển vào giỏ hàng." });
        }

        [HttpPost]
        public async Task<IActionResult> MoveAllToCart()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var wishlistItems = await _wishlistService.GetUserWishlistAsync(userId.Value);
            if (!wishlistItems.Any())
            {
                return Json(new { success = false, message = "Danh sách yêu thích trống." });
            }

            var successCount = 0;
            foreach (var item in wishlistItems)
            {
                var result = await _wishlistService.MoveToCartAsync(userId.Value, item.ProductId);
                if (result) successCount++;
            }

            if (successCount > 0)
            {
                return Json(new { success = true, message = $"Đã chuyển {successCount} sản phẩm vào giỏ hàng." });
            }
            else
            {
                return Json(new { success = false, message = "Không thể chuyển sản phẩm vào giỏ hàng." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { count = 0 });
            }

            var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> CheckInWishlist(int productId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { inWishlist = false });
            }

            var inWishlist = await _wishlistService.IsInWishlistAsync(userId.Value, productId);
            return Json(new { inWishlist });
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
    }
}
