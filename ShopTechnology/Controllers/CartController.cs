using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using ShopTechnology.Services;

namespace ShopTechnology.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly ICartService _cartService;

        public CartController(ShopTechnologyAccessoriesContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await GetOrCreateCartAsync(Guid.Parse(userId));
            var cartViewModel = await GetCartViewModelAsync(cart);

            return View(cartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var cart = await GetOrCreateCartAsync(Guid.Parse(userId));
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity
                    });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId.ToString() == userId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã cập nhật giỏ hàng" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId.ToString() == userId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var cart = await GetOrCreateCartAsync(Guid.Parse(userId));
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa giỏ hàng" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            try
            {
                var cart = await GetOrCreateCartAsync(Guid.Parse(userId));
                var count = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .SumAsync(ci => ci.Quantity);

                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { total = 0 });
            }

            try
            {
                var cart = await GetOrCreateCartAsync(Guid.Parse(userId));
                var total = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cart.CartId)
                    .SumAsync(ci => ci.Quantity * ci.Product.Price);

                return Json(new { total = Math.Round(total, 2) });
            }
            catch
            {
                return Json(new { total = 0 });
            }
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<CartViewModel> GetCartViewModelAsync(Cart cart)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();

            var items = cartItems.Select(ci => new CartItemViewModel
            {
                CartItemId = ci.CartItemId,
                ProductId = ci.ProductId,
                ProductName = ci.Product.ProductName,
                Price = ci.Product.Price,
                Quantity = ci.Quantity,
                Total = ci.Quantity * ci.Product.Price,
                ImageUrl = ci.Product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ??
                          ci.Product.ProductImages.FirstOrDefault()?.ImageUrl ?? string.Empty
            }).ToList();

            return new CartViewModel
            {
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                TotalAmount = items.Sum(i => i.Total)
            };
        }
    }
}
