using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ShopTechnologyAccessoriesContext context,
        ICartService cartService,
        IProductService productService,
        ILogger<CartController> logger)
    {
        _context = context;
        _cartService = cartService;
        _productService = productService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            var viewModel = new CartViewModel
            {
                CartItems = cart?.CartItems.ToList() ?? new List<CartItem>(),
                TotalAmount = cart?.CartItems.Sum(ci => ci.Product.Price * ci.Quantity) ?? 0
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cart for user");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Check if product exists and has enough stock
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Sản phẩm không đủ số lượng trong kho" });
            }

            var result = await _cartService.AddToCartAsync(userId.Value, productId, quantity);

            if (result)
            {
                var cartItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartItemCount });
            }
            else
            {
                return Json(new { success = false, message = "Không thể thêm vào giỏ hàng" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to cart");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }

            if (cartItem.Product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Sản phẩm không đủ số lượng trong kho" });
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            var totalAmount = await _cartService.GetCartTotalAsync(userId.Value);
            var itemTotal = cartItem.Product.Price * quantity;

            return Json(new
            {
                success = true,
                message = "Cập nhật thành công",
                itemTotal = itemTotal.ToString("N0"),
                totalAmount = totalAmount.ToString("N0")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item quantity");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var result = await _cartService.RemoveFromCartAsync(userId.Value, cartItemId);

            if (result)
            {
                var totalAmount = await _cartService.GetCartTotalAsync(userId.Value);
                var cartItemCount = await _cartService.GetCartItemCountAsync(userId.Value);

                return Json(new
                {
                    success = true,
                    message = "Đã xóa khỏi giỏ hàng",
                    totalAmount = totalAmount.ToString("N0"),
                    cartItemCount
                });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa sản phẩm" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var result = await _cartService.ClearCartAsync(userId.Value);

            if (result)
            {
                return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể xóa giỏ hàng" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ApplyPromotion(string promotionCode)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (string.IsNullOrWhiteSpace(promotionCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã khuyến mãi" });
            }

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == promotionCode && p.IsActive);

            if (promotion == null)
            {
                return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ" });
            }

            if (promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "Mã khuyến mãi đã hết hạn hoặc chưa có hiệu lực" });
            }

            var cartTotal = await _cartService.GetCartTotalAsync(userId.Value);
            if (cartTotal < promotion.MinimumOrderAmount)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đơn hàng tối thiểu {promotion.MinimumOrderAmount:N0} VNĐ để áp dụng mã này"
                });
            }

            if (promotion.UsedCount >= promotion.MaxUsageCount)
            {
                return Json(new { success = false, message = "Mã khuyến mãi đã hết lượt sử dụng" });
            }

            // Calculate discount
            decimal discountAmount = 0;
            if (promotion.DiscountPercentage > 0)
            {
                discountAmount = cartTotal * (promotion.DiscountPercentage / 100);
            }
            else
            {
                discountAmount = promotion.DiscountAmount;
            }

            var finalAmount = cartTotal - discountAmount;

            return Json(new
            {
                success = true,
                message = $"Áp dụng mã khuyến mãi thành công! Giảm {discountAmount:N0} VNĐ",
                discountAmount = discountAmount.ToString("N0"),
                finalAmount = finalAmount.ToString("N0"),
                promotionName = promotion.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying promotion");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCartSummary()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { cartItemCount = 0, totalAmount = 0 });
            }

            var cartItemCount = await _cartService.GetCartItemCountAsync(userId.Value);
            var totalAmount = await _cartService.GetCartTotalAsync(userId.Value);

            return Json(new
            {
                cartItemCount,
                totalAmount = totalAmount.ToString("N0")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart summary");
            return Json(new { cartItemCount = 0, totalAmount = 0 });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
