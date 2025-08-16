using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;
using System.Security.Claims;

namespace ShopTechnologyAccessories.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _db;

        public WishlistController(ShopTechnologyAccessoriesContext db)
        {
            _db = db;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var wishlistItems = await _db.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();
            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetUserId();
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Products");
            }

            var existingWishlistItem = await _db.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existingWishlistItem != null)
            {
                TempData["Info"] = "Sản phẩm đã có trong danh sách yêu thích.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId
            };

            _db.Wishlists.Add(wishlistItem);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Sản phẩm đã được thêm vào danh sách yêu thích.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = GetUserId();
            var wishlistItem = await _db.Wishlists.FindAsync(id);

            if (wishlistItem == null || wishlistItem.UserId != userId)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm trong danh sách yêu thích.";
                return RedirectToAction("Index", "Wishlist");
            }

            _db.Wishlists.Remove(wishlistItem);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Sản phẩm đã được xóa khỏi danh sách yêu thích.";
            return RedirectToAction("Index", "Wishlist");
        }
    }
}
