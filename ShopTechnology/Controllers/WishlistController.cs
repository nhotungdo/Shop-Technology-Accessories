using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItems = await _wishlistService.GetUserWishlistAsync(userId.Value);
            return View(wishlistItems);
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
