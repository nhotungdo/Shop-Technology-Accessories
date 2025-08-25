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

                // Create order using raw SQL to avoid navigation properties
                var orderNumber = GenerateOrderNumber();
                var orderSql = @"INSERT INTO Orders (UserId, OrderNumber, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, ShippingCity, ShippingProvince, ShippingPostalCode, OrderNotes, SubTotal, TaxAmount, ShippingFee, DiscountAmount, TotalAmount, OrderStatus, PaymentStatus, PaymentMethod, ShippingMethod, CreatedAt) 
                                VALUES (@UserId, @OrderNumber, @CustomerName, @CustomerEmail, @CustomerPhone, @ShippingAddress, @ShippingCity, @ShippingProvince, @ShippingPostalCode, @OrderNotes, @SubTotal, @TaxAmount, @ShippingFee, @DiscountAmount, @TotalAmount, @OrderStatus, @PaymentStatus, @PaymentMethod, @ShippingMethod, @CreatedAt);
                                SELECT CAST(SCOPE_IDENTITY() as int)";

                var orderId = await _context.Database.ExecuteSqlRawAsync(orderSql,
                    new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@OrderNumber", orderNumber),
                    new Microsoft.Data.SqlClient.SqlParameter("@CustomerName", model.CustomerName),
                    new Microsoft.Data.SqlClient.SqlParameter("@CustomerEmail", model.CustomerEmail),
                    new Microsoft.Data.SqlClient.SqlParameter("@CustomerPhone", model.CustomerPhone),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingAddress", model.ShippingAddress),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingCity", (object)model.ShippingCity ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingProvince", (object)model.ShippingProvince ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingPostalCode", (object)model.ShippingPostalCode ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@OrderNotes", (object)model.OrderNotes ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@SubTotal", cart.SubTotal),
                    new Microsoft.Data.SqlClient.SqlParameter("@TaxAmount", cart.TaxAmount),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingFee", cart.ShippingFee),
                    new Microsoft.Data.SqlClient.SqlParameter("@DiscountAmount", cart.DiscountAmount),
                    new Microsoft.Data.SqlClient.SqlParameter("@TotalAmount", cart.TotalAmount),
                    new Microsoft.Data.SqlClient.SqlParameter("@OrderStatus", "Pending"),
                    new Microsoft.Data.SqlClient.SqlParameter("@PaymentStatus", "Pending"),
                    new Microsoft.Data.SqlClient.SqlParameter("@PaymentMethod", model.PaymentMethod),
                    new Microsoft.Data.SqlClient.SqlParameter("@ShippingMethod", model.ShippingMethod),
                    new Microsoft.Data.SqlClient.SqlParameter("@CreatedAt", DateTime.Now));

                // Get the created order ID
                var order = new { OrderId = orderId };

                // Create order details using raw SQL to avoid navigation properties
                foreach (var item in cart.Items)
                {
                    var sql = @"INSERT INTO OrderDetails (OrderId, ProductId, ProductName, ProductSKU, Quantity, UnitPrice, TotalPrice, ProductImage, ProductBrand) 
                               VALUES (@OrderId, @ProductId, @ProductName, @ProductSKU, @Quantity, @UnitPrice, @TotalPrice, @ProductImage, @ProductBrand)";

                    await _context.Database.ExecuteSqlRawAsync(sql,
                        new Microsoft.Data.SqlClient.SqlParameter("@OrderId", order.OrderId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductId", item.ProductId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductName", item.ProductName),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductSKU", (object)item.ProductSKU ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Quantity", item.Quantity),
                        new Microsoft.Data.SqlClient.SqlParameter("@UnitPrice", item.UnitPrice),
                        new Microsoft.Data.SqlClient.SqlParameter("@TotalPrice", item.TotalPrice),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductImage", (object)item.ProductImage ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@ProductBrand", (object)item.ProductBrand ?? DBNull.Value));
                }

                // Create order history using raw SQL
                var historySql = @"INSERT INTO OrderHistories (OrderId, Status, Notes, CreatedAt) 
                                  VALUES (@OrderId, @Status, @Notes, @CreatedAt)";

                await _context.Database.ExecuteSqlRawAsync(historySql,
                    new Microsoft.Data.SqlClient.SqlParameter("@OrderId", order.OrderId),
                    new Microsoft.Data.SqlClient.SqlParameter("@Status", "Pending"),
                    new Microsoft.Data.SqlClient.SqlParameter("@Notes", "Đơn hàng được tạo"),
                    new Microsoft.Data.SqlClient.SqlParameter("@CreatedAt", DateTime.Now));

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
                payment.UpdatedAt = DateTime.Now;
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
