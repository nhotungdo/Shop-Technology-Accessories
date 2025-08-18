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
            var products = await _wishlistService.GetAllAsync(Guid.Parse(userIdStr));
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            await _wishlistService.AddAsync(Guid.Parse(userIdStr), productId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var ok = await _wishlistService.RemoveAsync(Guid.Parse(userIdStr), productId);
            return Json(new { success = ok });
        }
    }
}
