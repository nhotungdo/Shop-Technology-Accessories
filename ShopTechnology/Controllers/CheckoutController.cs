using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        ShopTechnologyAccessoriesContext context,
        ICartService cartService,
        IOrderService orderService,
        IEmailService emailService,
        ILogger<CheckoutController> logger)
    {
        _context = context;
        _cartService = cartService;
        _orderService = orderService;
        _emailService = emailService;
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
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Validate cart
            if (!await _cartService.ValidateCartAsync(userId.Value))
            {
                TempData["ErrorMessage"] = "Một số sản phẩm trong giỏ hàng không còn đủ số lượng";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            var viewModel = new CheckoutViewModel
            {
                CartItems = cart.CartItems.ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                User = user,
                ShippingAddress = user?.Address ?? "",
                City = user?.City ?? "",
                PostalCode = user?.PostalCode ?? ""
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkout page");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId.Value);
            if (cart == null || !cart.CartItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống" });
            }

            // Validate cart again
            if (!await _cartService.ValidateCartAsync(userId.Value))
            {
                return Json(new { success = false, message = "Một số sản phẩm không còn đủ số lượng" });
            }

            // Calculate total
            var totalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);

            // Apply promotion if any
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(model.PromotionCode))
            {
                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Code == model.PromotionCode && p.IsActive);

                if (promotion != null && promotion.StartDate <= DateTime.UtcNow &&
                    promotion.EndDate >= DateTime.UtcNow && promotion.UsedCount < promotion.MaxUsageCount)
                {
                    if (promotion.DiscountPercentage > 0)
                    {
                        discountAmount = totalAmount * (promotion.DiscountPercentage / 100);
                    }
                    else
                    {
                        discountAmount = promotion.DiscountAmount;
                    }
                }
            }

            var finalAmount = totalAmount - discountAmount;

            // Create order
            var orderItems = cart.CartItems.Select(ci => new OrderItemViewModel
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList();

            var createOrderModel = new CreateOrderViewModel
            {
                UserId = userId.Value,
                ShippingAddress = $"{model.ShippingAddress}, {model.City}, {model.PostalCode}",
                PaymentMethod = model.PaymentMethod,
                TotalAmount = finalAmount,
                OrderItems = orderItems
            };

            var order = await _orderService.CreateOrderAsync(createOrderModel);

            // Clear cart
            await _cartService.ClearCartAsync(userId.Value);

            // Update user address if provided
            if (!string.IsNullOrEmpty(model.ShippingAddress))
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.Address = model.ShippingAddress;
                    user.City = model.City;
                    user.PostalCode = model.PostalCode;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Order created successfully: {OrderId}", order.OrderId);

            return Json(new
            {
                success = true,
                message = "Đặt hàng thành công!",
                orderId = order.OrderId.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý đơn hàng" });
        }
    }

    public async Task<IActionResult> Confirmation(Guid orderId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order confirmation for ID: {OrderId}", orderId);
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ValidatePromotion(string promotionCode, decimal orderAmount)
    {
        try
        {
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

            if (orderAmount < promotion.MinimumOrderAmount)
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

            decimal discountAmount = 0;
            if (promotion.DiscountPercentage > 0)
            {
                discountAmount = orderAmount * (promotion.DiscountPercentage / 100);
            }
            else
            {
                discountAmount = promotion.DiscountAmount;
            }

            var finalAmount = orderAmount - discountAmount;

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
            _logger.LogError(ex, "Error validating promotion code: {PromotionCode}", promotionCode);
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentMethods()
    {
        try
        {
            var paymentMethods = new List<PaymentMethodViewModel>
            {
                new PaymentMethodViewModel { Id = "COD", Name = "Thanh toán khi nhận hàng (COD)", Description = "Thanh toán bằng tiền mặt khi nhận hàng" },
                new PaymentMethodViewModel { Id = "VNPAY", Name = "VNPay", Description = "Thanh toán qua VNPay" },
                new PaymentMethodViewModel { Id = "MOMO", Name = "MoMo", Description = "Thanh toán qua ví MoMo" },
                new PaymentMethodViewModel { Id = "BANK_TRANSFER", Name = "Chuyển khoản ngân hàng", Description = "Chuyển khoản trực tiếp vào tài khoản ngân hàng" }
            };

            return Json(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment methods");
            return Json(new { error = "Có lỗi xảy ra khi tải phương thức thanh toán" });
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

public class CheckoutViewModel
{
    public List<CartItem> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public User? User { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PromotionCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class PaymentMethodViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
