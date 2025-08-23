using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using System;

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
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            var products = await _wishlistService.GetWishlistProductsAsync(Guid.Parse(userIdStr));
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var result = await _wishlistService.AddToWishlistAsync(Guid.Parse(userIdStr), productId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var ok = await _wishlistService.RemoveFromWishlistAsync(Guid.Parse(userIdStr), productId);
            return Json(new { success = ok });
        }
    }
}
