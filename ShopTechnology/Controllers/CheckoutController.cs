using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public CheckoutController(
            ShopTechnologyAccessoriesContext context,
            ICartService cartService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _context = context;
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
            }

            var cart = await _cartService.GetCartAsync(userId);
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = await _context.Users.FindAsync(userId);
            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                CustomerPhone = user.PhoneNumber,
                ShippingAddress = user.Address,
                ShippingCity = user.City,
                ShippingProvince = user.Province,
                ShippingPostalCode = user.PostalCode
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var cart = await _cartService.GetCartAsync(userId);
                if (cart.Items.Count == 0)
                {
                    return RedirectToAction("Index", "Cart");
                }

                // Create order
                var order = new Order
                {
                    UserId = userId.Value,
                    OrderNumber = GenerateOrderNumber(),
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingProvince = model.ShippingProvince,
                    ShippingPostalCode = model.ShippingPostalCode,
                    OrderNotes = model.OrderNotes,
                    SubTotal = cart.SubTotal,
                    TaxAmount = cart.TaxAmount,
                    ShippingFee = cart.ShippingFee,
                    DiscountAmount = cart.DiscountAmount,
                    TotalAmount = cart.TotalAmount,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    PaymentMethod = model.PaymentMethod,
                    ShippingMethod = model.ShippingMethod,
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order details
                foreach (var item in cart.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductSKU = item.ProductSKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        ProductImage = item.ProductImage,
                        ProductBrand = item.ProductBrand
                    };

                    _context.OrderDetails.Add(orderDetail);
                }

                // Create order history
                var orderHistory = new OrderHistory
                {
                    OrderId = order.OrderId,
                    Status = "Pending",
                    Notes = "Đơn hàng được tạo",
                    CreatedAt = DateTime.Now
                };

                _context.OrderHistories.Add(orderHistory);
                await _context.SaveChangesAsync();

                // Clear cart
                await _cartService.ClearCartAsync(userId);

                // Redirect to payment
                return RedirectToAction("Payment", new { orderId = order.OrderId });
            }

            // If we got this far, something failed, redisplay form
            var cartForView = await _cartService.GetCartAsync(userId);
            model.Cart = cartForView;
            return View(model);
        }

        public async Task<IActionResult> Payment(int orderId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new PaymentViewModel
            {
                Order = order
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, string paymentMethod)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Create payment record
            var payment = new Payment
            {
                OrderId = orderId,
                PaymentMethod = paymentMethod,
                Amount = order.TotalAmount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Process payment based on method
            var paymentResult = await _paymentService.ProcessPaymentAsync(payment.PaymentId, paymentMethod);

            if (paymentResult.Success)
            {
                // Update order status
                order.PaymentStatus = "Paid";
                order.OrderStatus = "Processing";
                order.UpdatedAt = DateTime.Now;

                // Add order history
                var orderHistory = new OrderHistory
                {
                    OrderId = orderId,
                    Status = "Processing",
                    Notes = "Thanh toán thành công",
                    CreatedAt = DateTime.Now
                };

                _context.OrderHistories.Add(orderHistory);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { orderId = orderId });
            }
            else
            {
                // Update payment status
                payment.Status = "Failed";
                payment.ErrorMessage = paymentResult.Message;
                payment.ProcessedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = paymentResult.Message;
                return RedirectToAction("Payment", new { orderId = orderId });
            }
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int orderId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderHistories.OrderByDescending(oh => oh.CreatedAt))
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.Now:yyyyMMdd}{DateTime.Now:HHmmss}";
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
    }
}
