using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using System.Text.Json;

namespace ShopTechnology.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public CartController(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
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
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
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

                var cart = await GetCartViewModelAsync(cartItem.Cart);
                return Json(new
                {
                    success = true,
                    message = "Cập nhật thành công",
                    totalAmount = cart.TotalAmount,
                    totalItems = cart.TotalItems
                });
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

                var cart = await GetCartViewModelAsync(cartItem.Cart);
                return Json(new
                {
                    success = true,
                    message = "Đã xóa sản phẩm",
                    totalAmount = cart.TotalAmount,
                    totalItems = cart.TotalItems
                });
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
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId.ToString() == userId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa tất cả sản phẩm" });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<CartViewModel> GetCartViewModelAsync(Cart cart)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Product.ProductImages)
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();

            var cartViewModel = new CartViewModel
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = cartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.ProductName,
                    ProductImage = ci.Product.ProductImages?.FirstOrDefault(pi => pi.IsMain)?.ImageUrl ?? "",
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    StockQuantity = ci.Product.StockQuantity
                }).ToList()
            };

            return cartViewModel;
        }
    }
}
