using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;
using System.Data;

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
                var historySql = @"INSERT INTO OrderHistory (OrderId, Status, Notes, CreatedAt) 
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

            // Use ADO.NET directly to avoid Entity Framework navigation property issues
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get order
            var orderSql = @"SELECT OrderId, OrderNumber, UserId, CustomerName, CustomerEmail, CustomerPhone, 
                                   ShippingAddress, ShippingCity, ShippingProvince, ShippingPostalCode, OrderNotes,
                                   SubTotal, TaxAmount, ShippingFee, DiscountAmount, TotalAmount, OrderStatus, 
                                   PaymentStatus, PaymentMethod, TrackingNumber, ShippingMethod, 
                                   EstimatedDeliveryDate, ShippedDate, DeliveredDate, CreatedAt, UpdatedAt
                            FROM Orders 
                            WHERE OrderId = @OrderId AND UserId = @UserId";

            using var orderCommand = connection.CreateCommand();
            orderCommand.CommandText = orderSql;
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId));

            Order order = null;
            using (var reader = await orderCommand.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    order = new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                        CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
                        ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                        ShippingCity = reader.IsDBNull(reader.GetOrdinal("ShippingCity")) ? null : reader.GetString(reader.GetOrdinal("ShippingCity")),
                        ShippingProvince = reader.IsDBNull(reader.GetOrdinal("ShippingProvince")) ? null : reader.GetString(reader.GetOrdinal("ShippingProvince")),
                        ShippingPostalCode = reader.IsDBNull(reader.GetOrdinal("ShippingPostalCode")) ? null : reader.GetString(reader.GetOrdinal("ShippingPostalCode")),
                        OrderNotes = reader.IsDBNull(reader.GetOrdinal("OrderNotes")) ? null : reader.GetString(reader.GetOrdinal("OrderNotes")),
                        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                        TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                        ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                        DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                        PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        TrackingNumber = reader.IsDBNull(reader.GetOrdinal("TrackingNumber")) ? null : reader.GetString(reader.GetOrdinal("TrackingNumber")),
                        ShippingMethod = reader.IsDBNull(reader.GetOrdinal("ShippingMethod")) ? null : reader.GetString(reader.GetOrdinal("ShippingMethod")),
                        EstimatedDeliveryDate = reader.IsDBNull(reader.GetOrdinal("EstimatedDeliveryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EstimatedDeliveryDate")),
                        ShippedDate = reader.IsDBNull(reader.GetOrdinal("ShippedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ShippedDate")),
                        DeliveredDate = reader.IsDBNull(reader.GetOrdinal("DeliveredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DeliveredDate")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    };
                }
            }

            if (order == null)
            {
                return NotFound();
            }

            // Get order details
            var orderDetailsSql = @"SELECT OrderDetailId, OrderId, ProductId, ProductName, ProductSKU, 
                                          Quantity, UnitPrice, TotalPrice, ProductImage, ProductBrand
                                   FROM OrderDetails 
                                   WHERE OrderId = @OrderId";

            using var detailsCommand = connection.CreateCommand();
            detailsCommand.CommandText = orderDetailsSql;
            detailsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));

            var orderDetails = new List<OrderDetail>();
            using (var reader = await detailsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderDetailId = reader.GetInt32(reader.GetOrdinal("OrderDetailId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        ProductSKU = reader.IsDBNull(reader.GetOrdinal("ProductSKU")) ? null : reader.GetString(reader.GetOrdinal("ProductSKU")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        ProductImage = reader.IsDBNull(reader.GetOrdinal("ProductImage")) ? null : reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductBrand = reader.IsDBNull(reader.GetOrdinal("ProductBrand")) ? null : reader.GetString(reader.GetOrdinal("ProductBrand"))
                    });
                }
            }

            // Set order details
            order.OrderDetails = orderDetails;

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

            // Use ADO.NET directly to avoid Entity Framework navigation property issues
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get order
            var orderSql = @"SELECT OrderId, OrderNumber, UserId, CustomerName, CustomerEmail, CustomerPhone, 
                                   ShippingAddress, ShippingCity, ShippingProvince, ShippingPostalCode, OrderNotes,
                                   SubTotal, TaxAmount, ShippingFee, DiscountAmount, TotalAmount, OrderStatus, 
                                   PaymentStatus, PaymentMethod, TrackingNumber, ShippingMethod, 
                                   EstimatedDeliveryDate, ShippedDate, DeliveredDate, CreatedAt, UpdatedAt
                            FROM Orders 
                            WHERE OrderId = @OrderId AND UserId = @UserId";

            using var orderCommand = connection.CreateCommand();
            orderCommand.CommandText = orderSql;
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId));

            Order order = null;
            using (var reader = await orderCommand.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    order = new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                        CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
                        ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                        ShippingCity = reader.IsDBNull(reader.GetOrdinal("ShippingCity")) ? null : reader.GetString(reader.GetOrdinal("ShippingCity")),
                        ShippingProvince = reader.IsDBNull(reader.GetOrdinal("ShippingProvince")) ? null : reader.GetString(reader.GetOrdinal("ShippingProvince")),
                        ShippingPostalCode = reader.IsDBNull(reader.GetOrdinal("ShippingPostalCode")) ? null : reader.GetString(reader.GetOrdinal("ShippingPostalCode")),
                        OrderNotes = reader.IsDBNull(reader.GetOrdinal("OrderNotes")) ? null : reader.GetString(reader.GetOrdinal("OrderNotes")),
                        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                        TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                        ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                        DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                        PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        TrackingNumber = reader.IsDBNull(reader.GetOrdinal("TrackingNumber")) ? null : reader.GetString(reader.GetOrdinal("TrackingNumber")),
                        ShippingMethod = reader.IsDBNull(reader.GetOrdinal("ShippingMethod")) ? null : reader.GetString(reader.GetOrdinal("ShippingMethod")),
                        EstimatedDeliveryDate = reader.IsDBNull(reader.GetOrdinal("EstimatedDeliveryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EstimatedDeliveryDate")),
                        ShippedDate = reader.IsDBNull(reader.GetOrdinal("ShippedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ShippedDate")),
                        DeliveredDate = reader.IsDBNull(reader.GetOrdinal("DeliveredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DeliveredDate")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    };
                }
            }

            if (order == null)
            {
                return NotFound();
            }

            // Get order details
            var orderDetailsSql = @"SELECT OrderDetailId, OrderId, ProductId, ProductName, ProductSKU, 
                                          Quantity, UnitPrice, TotalPrice, ProductImage, ProductBrand
                                   FROM OrderDetails 
                                   WHERE OrderId = @OrderId";

            using var detailsCommand = connection.CreateCommand();
            detailsCommand.CommandText = orderDetailsSql;
            detailsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));

            var orderDetails = new List<OrderDetail>();
            using (var reader = await detailsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderDetailId = reader.GetInt32(reader.GetOrdinal("OrderDetailId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        ProductSKU = reader.IsDBNull(reader.GetOrdinal("ProductSKU")) ? null : reader.GetString(reader.GetOrdinal("ProductSKU")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        ProductImage = reader.IsDBNull(reader.GetOrdinal("ProductImage")) ? null : reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductBrand = reader.IsDBNull(reader.GetOrdinal("ProductBrand")) ? null : reader.GetString(reader.GetOrdinal("ProductBrand"))
                    });
                }
            }

            // Set order details
            order.OrderDetails = orderDetails;

            return View(order);
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Use ADO.NET directly to avoid Entity Framework navigation property issues
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var ordersSql = @"SELECT OrderId, OrderNumber, UserId, CustomerName, CustomerEmail, CustomerPhone, 
                                   ShippingAddress, ShippingCity, ShippingProvince, ShippingPostalCode, OrderNotes,
                                   SubTotal, TaxAmount, ShippingFee, DiscountAmount, TotalAmount, OrderStatus, 
                                   PaymentStatus, PaymentMethod, TrackingNumber, ShippingMethod, 
                                   EstimatedDeliveryDate, ShippedDate, DeliveredDate, CreatedAt, UpdatedAt
                            FROM Orders 
                            WHERE UserId = @UserId
                            ORDER BY CreatedAt DESC";

            using var ordersCommand = connection.CreateCommand();
            ordersCommand.CommandText = ordersSql;
            ordersCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId));

            var orders = new List<Order>();
            using (var reader = await ordersCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var order = new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                        CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
                        ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                        ShippingCity = reader.IsDBNull(reader.GetOrdinal("ShippingCity")) ? null : reader.GetString(reader.GetOrdinal("ShippingCity")),
                        ShippingProvince = reader.IsDBNull(reader.GetOrdinal("ShippingProvince")) ? null : reader.GetString(reader.GetOrdinal("ShippingProvince")),
                        ShippingPostalCode = reader.IsDBNull(reader.GetOrdinal("ShippingPostalCode")) ? null : reader.GetString(reader.GetOrdinal("ShippingPostalCode")),
                        OrderNotes = reader.IsDBNull(reader.GetOrdinal("OrderNotes")) ? null : reader.GetString(reader.GetOrdinal("OrderNotes")),
                        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                        TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                        ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                        DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                        PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        TrackingNumber = reader.IsDBNull(reader.GetOrdinal("TrackingNumber")) ? null : reader.GetString(reader.GetOrdinal("TrackingNumber")),
                        ShippingMethod = reader.IsDBNull(reader.GetOrdinal("ShippingMethod")) ? null : reader.GetString(reader.GetOrdinal("ShippingMethod")),
                        EstimatedDeliveryDate = reader.IsDBNull(reader.GetOrdinal("EstimatedDeliveryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EstimatedDeliveryDate")),
                        ShippedDate = reader.IsDBNull(reader.GetOrdinal("ShippedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ShippedDate")),
                        DeliveredDate = reader.IsDBNull(reader.GetOrdinal("DeliveredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DeliveredDate")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    };
                    orders.Add(order);
                }
            }

            // Get order details for all orders
            foreach (var order in orders)
            {
                var orderDetailsSql = @"SELECT OrderDetailId, OrderId, ProductId, ProductName, ProductSKU, 
                                              Quantity, UnitPrice, TotalPrice, ProductImage, ProductBrand
                                       FROM OrderDetails 
                                       WHERE OrderId = @OrderId";

                using var detailsCommand = connection.CreateCommand();
                detailsCommand.CommandText = orderDetailsSql;
                detailsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", order.OrderId));

                var orderDetails = new List<OrderDetail>();
                using (var reader = await detailsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orderDetails.Add(new OrderDetail
                        {
                            OrderDetailId = reader.GetInt32(reader.GetOrdinal("OrderDetailId")),
                            OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            ProductSKU = reader.IsDBNull(reader.GetOrdinal("ProductSKU")) ? null : reader.GetString(reader.GetOrdinal("ProductSKU")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                            TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                            ProductImage = reader.IsDBNull(reader.GetOrdinal("ProductImage")) ? null : reader.GetString(reader.GetOrdinal("ProductImage")),
                            ProductBrand = reader.IsDBNull(reader.GetOrdinal("ProductBrand")) ? null : reader.GetString(reader.GetOrdinal("ProductBrand"))
                        });
                    }
                }
                order.OrderDetails = orderDetails;
            }

            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int orderId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Use ADO.NET directly to avoid Entity Framework navigation property issues
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            // Get order
            var orderSql = @"SELECT OrderId, OrderNumber, UserId, CustomerName, CustomerEmail, CustomerPhone, 
                                   ShippingAddress, ShippingCity, ShippingProvince, ShippingPostalCode, OrderNotes,
                                   SubTotal, TaxAmount, ShippingFee, DiscountAmount, TotalAmount, OrderStatus, 
                                   PaymentStatus, PaymentMethod, TrackingNumber, ShippingMethod, 
                                   EstimatedDeliveryDate, ShippedDate, DeliveredDate, CreatedAt, UpdatedAt
                            FROM Orders 
                            WHERE OrderId = @OrderId AND UserId = @UserId";

            using var orderCommand = connection.CreateCommand();
            orderCommand.CommandText = orderSql;
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));
            orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId));

            Order order = null;
            using (var reader = await orderCommand.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    order = new Order
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                        CustomerPhone = reader.GetString(reader.GetOrdinal("CustomerPhone")),
                        ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                        ShippingCity = reader.IsDBNull(reader.GetOrdinal("ShippingCity")) ? null : reader.GetString(reader.GetOrdinal("ShippingCity")),
                        ShippingProvince = reader.IsDBNull(reader.GetOrdinal("ShippingProvince")) ? null : reader.GetString(reader.GetOrdinal("ShippingProvince")),
                        ShippingPostalCode = reader.IsDBNull(reader.GetOrdinal("ShippingPostalCode")) ? null : reader.GetString(reader.GetOrdinal("ShippingPostalCode")),
                        OrderNotes = reader.IsDBNull(reader.GetOrdinal("OrderNotes")) ? null : reader.GetString(reader.GetOrdinal("OrderNotes")),
                        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
                        TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                        ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                        DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                        PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        TrackingNumber = reader.IsDBNull(reader.GetOrdinal("TrackingNumber")) ? null : reader.GetString(reader.GetOrdinal("TrackingNumber")),
                        ShippingMethod = reader.IsDBNull(reader.GetOrdinal("ShippingMethod")) ? null : reader.GetString(reader.GetOrdinal("ShippingMethod")),
                        EstimatedDeliveryDate = reader.IsDBNull(reader.GetOrdinal("EstimatedDeliveryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EstimatedDeliveryDate")),
                        ShippedDate = reader.IsDBNull(reader.GetOrdinal("ShippedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ShippedDate")),
                        DeliveredDate = reader.IsDBNull(reader.GetOrdinal("DeliveredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DeliveredDate")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    };
                }
            }

            if (order == null)
            {
                return NotFound();
            }

            // Get order details
            var orderDetailsSql = @"SELECT OrderDetailId, OrderId, ProductId, ProductName, ProductSKU, 
                                          Quantity, UnitPrice, TotalPrice, ProductImage, ProductBrand
                                   FROM OrderDetails 
                                   WHERE OrderId = @OrderId";

            using var detailsCommand = connection.CreateCommand();
            detailsCommand.CommandText = orderDetailsSql;
            detailsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));

            var orderDetails = new List<OrderDetail>();
            using (var reader = await detailsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderDetailId = reader.GetInt32(reader.GetOrdinal("OrderDetailId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                        ProductSKU = reader.IsDBNull(reader.GetOrdinal("ProductSKU")) ? null : reader.GetString(reader.GetOrdinal("ProductSKU")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        ProductImage = reader.IsDBNull(reader.GetOrdinal("ProductImage")) ? null : reader.GetString(reader.GetOrdinal("ProductImage")),
                        ProductBrand = reader.IsDBNull(reader.GetOrdinal("ProductBrand")) ? null : reader.GetString(reader.GetOrdinal("ProductBrand"))
                    });
                }
            }
            order.OrderDetails = orderDetails;

            // Get order history
            var orderHistorySql = @"SELECT OrderHistoryId, OrderId, Status, Notes, UpdatedByUserId, CreatedAt
                                   FROM OrderHistory 
                                   WHERE OrderId = @OrderId
                                   ORDER BY CreatedAt DESC";

            using var historyCommand = connection.CreateCommand();
            historyCommand.CommandText = orderHistorySql;
            historyCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));

            var orderHistories = new List<OrderHistory>();
            using (var reader = await historyCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orderHistories.Add(new OrderHistory
                    {
                        OrderHistoryId = reader.GetInt32(reader.GetOrdinal("OrderHistoryId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                        UpdatedByUserId = reader.IsDBNull(reader.GetOrdinal("UpdatedByUserId")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedByUserId")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
            }
            order.OrderHistories = orderHistories;

            // Get payments
            var paymentsSql = @"SELECT PaymentId, OrderId, PaymentMethod, PaymentProvider, TransactionId, Status, 
                                      Amount, Description, ErrorMessage, PaymentUrl, CallbackData, CreatedAt, UpdatedAt
                               FROM Payments 
                               WHERE OrderId = @OrderId";

            using var paymentsCommand = connection.CreateCommand();
            paymentsCommand.CommandText = paymentsSql;
            paymentsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId));

            var payments = new List<Payment>();
            using (var reader = await paymentsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    payments.Add(new Payment
                    {
                        PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                        OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        PaymentProvider = reader.IsDBNull(reader.GetOrdinal("PaymentProvider")) ? null : reader.GetString(reader.GetOrdinal("PaymentProvider")),
                        TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionId")) ? null : reader.GetString(reader.GetOrdinal("TransactionId")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage")),
                        PaymentUrl = reader.IsDBNull(reader.GetOrdinal("PaymentUrl")) ? null : reader.GetString(reader.GetOrdinal("PaymentUrl")),
                        CallbackData = reader.IsDBNull(reader.GetOrdinal("CallbackData")) ? null : reader.GetString(reader.GetOrdinal("CallbackData")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    });
                }
            }
            order.Payments = payments;

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
