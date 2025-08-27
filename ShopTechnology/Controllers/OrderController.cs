using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;
using System.Data;

namespace ShopTechnology.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IOrderService _orderService;

        public OrderController(ShopTechnologyAccessoriesContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
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
                ordersCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", int.Parse(userId)));

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

                var orderViewModels = orders.Select(order => new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedAt,
                    Status = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel
                    {
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ProductImage = od.ProductImage ?? "/img/best-tech-accessories.png"
                    }).ToList()
                }).ToList();

                // Debug logging
                Console.WriteLine($"Found {orders.Count} orders for user {userId}");
                foreach (var order in orders)
                {
                    Console.WriteLine($"Order {order.OrderId}: {order.OrderDetails.Count} items");
                }

                return View(orderViewModels);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading order history: {ex.Message}");
                return View(new List<OrderViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
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
                orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", id));
                orderCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", int.Parse(userId)));

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
                detailsCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", id));

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

                // Debug logging
                Console.WriteLine($"Order {order.OrderId} has {orderDetails.Count} items");

                // Get order history
                var orderHistorySql = @"SELECT OrderHistoryId, OrderId, Status, Notes, UpdatedByUserId, CreatedAt
                                       FROM OrderHistory 
                                       WHERE OrderId = @OrderId
                                       ORDER BY CreatedAt DESC";

                using var historyCommand = connection.CreateCommand();
                historyCommand.CommandText = orderHistorySql;
                historyCommand.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@OrderId", id));

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

                var orderDetailViewModels = order.OrderDetails.Select(od => new OrderDetailViewModel
                {
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    Quantity = od.Quantity,
                    Price = od.UnitPrice,
                    ProductImage = od.ProductImage ?? "/img/best-tech-accessories.png"
                }).ToList();

                // Debug logging
                Console.WriteLine($"Created {orderDetailViewModels.Count} OrderDetailViewModels");

                var orderViewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedAt,
                    Status = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    CustomerPhone = order.CustomerPhone,
                    OrderDetails = orderDetailViewModels,
                    OrderHistories = order.OrderHistories?.Select(oh => new OrderHistoryViewModel
                    {
                        Status = oh.Status,
                        CreatedAt = oh.CreatedAt,
                        Note = oh.Notes ?? string.Empty
                    }).OrderByDescending(oh => oh.CreatedAt).ToList() ?? new List<OrderHistoryViewModel>()
                };

                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order details: {ex.Message}");
                return View("Error");
            }
        }
    }
}
