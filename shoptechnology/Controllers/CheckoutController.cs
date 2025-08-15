using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnologyAccessories.Services;
using System.Security.Claims;

namespace ShopTechnologyAccessories.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;
    private readonly IPaymentService _payPal;
    private readonly VNPayService _vnpay;

    public CheckoutController(
        ShopTechnologyAccessoriesContext db,
        IPaymentService payPal,
        VNPayService vnpay)
    {
        _db = db;
        _payPal = payPal;
        _vnpay = vnpay;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng trống";
            return RedirectToAction("Index", "Cart");
        }
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> PayByPayPal(string shippingAddress)
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any()) return RedirectToAction("Index", "Cart");

        var total = cart.CartItems.Sum(i => i.Quantity * i.Product.Price);
        var returnUrl = Url.Action("PayPalReturn", "Checkout", null, Request.Scheme)!;
        var cancelUrl = Url.Action("Index", "Cart", null, Request.Scheme)!;
        var url = await _payPal.CreatePaymentAsync(total, "USD", returnUrl, cancelUrl);
        TempData["ShippingAddress"] = shippingAddress;
        return Redirect(url);
    }

    [HttpGet]
    public async Task<IActionResult> PayPalReturn(string paymentId, string token, string payerId)
    {
        var ok = await _payPal.CapturePaymentAsync(paymentId, payerId);
        if (!ok) return RedirectToAction("Index", "Cart");
        return await CreateOrderAndClearCart("PayPal");
    }

    [HttpPost]
    public async Task<IActionResult> PayByVNPay(string shippingAddress)
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any()) return RedirectToAction("Index", "Cart");
        var total = cart.CartItems.Sum(i => i.Quantity * i.Product.Price);
        var returnUrl = Url.Action("VNPayReturn", "Checkout", null, Request.Scheme)!;
        var url = await _vnpay.CreatePaymentUrlAsync(total, returnUrl);
        TempData["ShippingAddress"] = shippingAddress;
        return Redirect(url);
    }

    [HttpGet]
    public async Task<IActionResult> VNPayReturn(string vnp_ResponseCode, string vnp_TxnRef)
    {
        if (vnp_ResponseCode != "00") return RedirectToAction("Index", "Cart");
        return await CreateOrderAndClearCart("VNPay");
    }

    private async Task<IActionResult> CreateOrderAndClearCart(string method)
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.CartItems.Any()) return RedirectToAction("Index", "Cart");

        var total = cart.CartItems.Sum(i => i.Quantity * i.Product.Price);

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            Method = method,
            Amount = total,
            PaymentDate = DateTime.UtcNow,
            Status = "Completed"
        };
        _db.Payments.Add(payment);

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            Status = "Pending",
            PaymentId = payment.PaymentId,
            ShippingAddress = (TempData["ShippingAddress"] as string) ?? string.Empty
        };
        _db.Orders.Add(order);

        foreach (var item in cart.CartItems)
        {
            _db.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price
            });
            item.Product.StockQuantity -= item.Quantity;
        }

        _db.CartItems.RemoveRange(cart.CartItems);
        await _db.SaveChangesAsync();
        TempData["success"] = "Thanh toán thành công!";
        return RedirectToAction("Details", "Orders", new { id = order.OrderId });
    }
}


