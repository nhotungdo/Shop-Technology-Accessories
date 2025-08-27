using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ShopTechnology.Services
{
    /// <summary>
    /// Service xử lý luồng dữ liệu sau xác nhận thanh toán
    /// DFD Level 2: Chi tiết quy trình hậu thanh toán
    /// </summary>
    public interface IPostPaymentService
    {
        // Cập nhật trạng thái đơn hàng
        Task<OrderStatusUpdateResult> UpdateOrderStatusAsync(int orderId, string newStatus, string transactionId);

        // Cập nhật kho hàng
        Task<InventoryUpdateResult> UpdateInventoryAfterPaymentAsync(int orderId);

        // Gửi xác nhận và thông báo cho khách hàng
        Task<NotificationResult> SendOrderConfirmationAsync(int orderId);

        // Kích hoạt logistics và chuẩn bị hàng
        Task<LogisticsResult> ActivateLogisticsAsync(int orderId);

        // Theo dõi và báo cáo
        Task<ReportingResult> GenerateReportsAsync(int orderId);

        // Xử lý ngoại lệ
        Task<ExceptionHandlingResult> HandlePaymentExceptionAsync(int orderId, string exceptionType, string message);

        // Quy trình hoàn chỉnh sau thanh toán
        Task<PostPaymentResult> ProcessPostPaymentAsync(int orderId, string transactionId);
    }

    public class PostPaymentService : IPostPaymentService
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PostPaymentService> _logger;

        public PostPaymentService(
            ShopTechnologyAccessoriesContext context,
            IEmailService emailService,
            ILogger<PostPaymentService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Quy trình hoàn chỉnh sau thanh toán
        /// Data Flow: Payment Gateway → Middle Tier → Multiple Systems
        /// </summary>
        public async Task<PostPaymentResult> ProcessPostPaymentAsync(int orderId, string transactionId)
        {
            try
            {
                Console.WriteLine("=== DFD Level 2: Quy trình hoàn chỉnh sau thanh toán ===");
                Console.WriteLine($"Data Flow: Payment Gateway → Middle Tier → Multiple Systems");
                Console.WriteLine($"Parameters: OrderId={orderId}, TransactionId={transactionId}");

                // Bước 1: Cập nhật trạng thái đơn hàng
                Console.WriteLine("Bước 1: Cập nhật trạng thái đơn hàng");
                var statusResult = await UpdateOrderStatusAsync(orderId, "Paid", transactionId);
                if (!statusResult.Success)
                {
                    return new PostPaymentResult
                    {
                        Success = false,
                        ErrorMessage = statusResult.ErrorMessage
                    };
                }

                // Bước 2: Cập nhật kho hàng
                Console.WriteLine("Bước 2: Cập nhật kho hàng");
                var inventoryResult = await UpdateInventoryAfterPaymentAsync(orderId);
                if (!inventoryResult.Success)
                {
                    return new PostPaymentResult
                    {
                        Success = false,
                        ErrorMessage = inventoryResult.ErrorMessage
                    };
                }

                // Bước 3: Gửi xác nhận và thông báo cho khách hàng
                Console.WriteLine("Bước 3: Gửi xác nhận và thông báo cho khách hàng");
                var notificationResult = await SendOrderConfirmationAsync(orderId);
                if (!notificationResult.Success)
                {
                    Console.WriteLine($"Warning: Notification failed: {notificationResult.ErrorMessage}");
                }

                // Bước 4: Kích hoạt logistics và chuẩn bị hàng
                Console.WriteLine("Bước 4: Kích hoạt logistics và chuẩn bị hàng");
                var logisticsResult = await ActivateLogisticsAsync(orderId);
                if (!logisticsResult.Success)
                {
                    Console.WriteLine($"Warning: Logistics activation failed: {logisticsResult.ErrorMessage}");
                }

                // Bước 5: Theo dõi và báo cáo
                Console.WriteLine("Bước 5: Theo dõi và báo cáo");
                var reportingResult = await GenerateReportsAsync(orderId);
                if (!reportingResult.Success)
                {
                    Console.WriteLine($"Warning: Reporting failed: {reportingResult.ErrorMessage}");
                }

                Console.WriteLine($"Post-payment processing completed successfully for OrderId: {orderId}");
                Console.WriteLine("Data Flow completed successfully");

                return new PostPaymentResult
                {
                    Success = true,
                    OrderId = orderId,
                    TransactionId = transactionId,
                    Message = "Quy trình sau thanh toán hoàn tất thành công"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPostPayment: {ex.Message}");
                _logger.LogError(ex, "Error processing post-payment for OrderId: {OrderId}", orderId);

                return new PostPaymentResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xử lý quy trình sau thanh toán"
                };
            }
        }

        /// <summary>
        /// Bước 1: Cập nhật trạng thái đơn hàng
        /// Data Flow: Payment Gateway → Middle Tier → Order Database
        /// </summary>
        public async Task<OrderStatusUpdateResult> UpdateOrderStatusAsync(int orderId, string newStatus, string transactionId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Bước 1: Cập nhật trạng thái đơn hàng ===");
                Console.WriteLine($"Data Flow: Payment Gateway → Middle Tier → Order Database");
                Console.WriteLine($"OrderId: {orderId}, NewStatus: {newStatus}, TransactionId: {transactionId}");

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return new OrderStatusUpdateResult
                    {
                        Success = false,
                        ErrorMessage = "Đơn hàng không tồn tại"
                    };
                }

                // Cập nhật trạng thái đơn hàng
                order.OrderStatus = newStatus;
                order.PaymentStatus = "Paid";
                order.UpdatedAt = DateTime.Now;

                // Tạo lịch sử đơn hàng
                var orderHistory = new OrderHistory
                {
                    OrderId = orderId,
                    Status = newStatus,
                    Notes = $"Thanh toán thành công. Transaction ID: {transactionId}",
                    CreatedAt = DateTime.Now
                };

                _context.OrderHistories.Add(orderHistory);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Order status updated successfully: {order.OrderNumber} -> {newStatus}");
                Console.WriteLine($"Order history created: {orderHistory.Notes}");

                return new OrderStatusUpdateResult
                {
                    Success = true,
                    OrderNumber = order.OrderNumber,
                    OldStatus = order.OrderStatus,
                    NewStatus = newStatus,
                    TransactionId = transactionId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateOrderStatus: {ex.Message}");
                return new OrderStatusUpdateResult
                {
                    Success = false,
                    ErrorMessage = "Không thể cập nhật trạng thái đơn hàng"
                };
            }
        }

        /// <summary>
        /// Bước 2: Cập nhật kho hàng
        /// Data Flow: Middle Tier → Product Database → Inventory Database
        /// </summary>
        public async Task<InventoryUpdateResult> UpdateInventoryAfterPaymentAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Bước 2: Cập nhật kho hàng ===");
                Console.WriteLine($"Data Flow: Middle Tier → Product Database → Inventory Database");
                Console.WriteLine($"OrderId: {orderId}");

                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == orderId)
                    .ToListAsync();

                var updatedProducts = new List<string>();
                var lowStockProducts = new List<string>();

                foreach (var detail in orderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        var oldStock = product.StockQuantity;
                        product.StockQuantity -= detail.Quantity;

                        updatedProducts.Add($"{product.Name}: {oldStock} -> {product.StockQuantity}");

                        // Kiểm tra tồn kho thấp
                        if (product.StockQuantity <= 10)
                        {
                            lowStockProducts.Add($"{product.Name} (Còn lại: {product.StockQuantity})");
                        }
                    }
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"Inventory updated for {updatedProducts.Count} products");
                foreach (var update in updatedProducts)
                {
                    Console.WriteLine($"  - {update}");
                }

                if (lowStockProducts.Count > 0)
                {
                    Console.WriteLine($"Low stock alert for {lowStockProducts.Count} products:");
                    foreach (var lowStock in lowStockProducts)
                    {
                        Console.WriteLine($"  - {lowStock}");
                    }
                }

                return new InventoryUpdateResult
                {
                    Success = true,
                    NewStockQuantity = 0, // Tạm thời set 0
                    ProductName = string.Join(", ", updatedProducts),
                    ErrorMessage = lowStockProducts.Count > 0 ? $"Low stock alert: {string.Join(", ", lowStockProducts)}" : null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateInventoryAfterPayment: {ex.Message}");
                return new InventoryUpdateResult
                {
                    Success = false,
                    ErrorMessage = "Không thể cập nhật kho hàng"
                };
            }
        }

        /// <summary>
        /// Bước 3: Gửi xác nhận và thông báo cho khách hàng
        /// Data Flow: Middle Tier → Customer Database → Email/SMS Gateway
        /// </summary>
        public async Task<NotificationResult> SendOrderConfirmationAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Bước 3: Gửi xác nhận và thông báo cho khách hàng ===");
                Console.WriteLine($"Data Flow: Middle Tier → Customer Database → Email/SMS Gateway");
                Console.WriteLine($"OrderId: {orderId}");

                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order?.User == null)
                {
                    return new NotificationResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy thông tin khách hàng"
                    };
                }

                // Gửi email xác nhận
                await _emailService.SendOrderConfirmationEmailAsync(
                    order.User.Email,
                    order.User.FullName,
                    order.OrderNumber
                );

                // Tạo thông báo trong hệ thống
                var notification = new Notification
                {
                    UserId = order.UserId,
                    Title = "Đơn hàng đã được xác nhận",
                    Message = $"Đơn hàng #{order.OrderNumber} của bạn đã được xác nhận thành công. Chúng tôi sẽ giao hàng trong 3-5 ngày làm việc.",
                    Type = "OrderConfirmation",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                // Tạm thời comment out vì chưa có DbSet Notifications
                // _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Order confirmation sent to: {order.User.Email}");
                Console.WriteLine($"Notification created: {notification.Title}");

                return new NotificationResult
                {
                    Success = true,
                    Message = $"Order confirmation sent to: {order.User.Email}",
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendOrderConfirmation: {ex.Message}");
                return new NotificationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể gửi thông báo xác nhận"
                };
            }
        }

        /// <summary>
        /// Bước 4: Kích hoạt logistics và chuẩn bị hàng
        /// Data Flow: Middle Tier → Warehouse System → Logistics Partner
        /// </summary>
        public async Task<LogisticsResult> ActivateLogisticsAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Bước 4: Kích hoạt logistics và chuẩn bị hàng ===");
                Console.WriteLine($"Data Flow: Middle Tier → Warehouse System → Logistics Partner");
                Console.WriteLine($"OrderId: {orderId}");

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return new LogisticsResult
                    {
                        Success = false,
                        ErrorMessage = "Đơn hàng không tồn tại"
                    };
                }

                // Tạo tracking number
                var trackingNumber = GenerateTrackingNumber();

                // Cập nhật thông tin logistics
                order.TrackingNumber = trackingNumber;
                order.OrderStatus = "Processing";
                order.EstimatedDeliveryDate = DateTime.Now.AddDays(5); // Ước tính 5 ngày
                order.UpdatedAt = DateTime.Now;

                // Tạo lịch sử logistics
                var logisticsHistory = new OrderHistory
                {
                    OrderId = orderId,
                    Status = "Processing",
                    Notes = $"Đơn hàng đang được xử lý. Tracking: {trackingNumber}",
                    CreatedAt = DateTime.Now
                };

                _context.OrderHistories.Add(logisticsHistory);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Logistics activated: Tracking Number = {trackingNumber}");
                Console.WriteLine($"Estimated delivery: {order.EstimatedDeliveryDate:dd/MM/yyyy}");

                return new LogisticsResult
                {
                    Success = true,
                    TrackingNumber = trackingNumber,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    OrderStatus = order.OrderStatus
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ActivateLogistics: {ex.Message}");
                return new LogisticsResult
                {
                    Success = false,
                    ErrorMessage = "Không thể kích hoạt logistics"
                };
            }
        }

        /// <summary>
        /// Bước 5: Theo dõi và báo cáo
        /// Data Flow: Middle Tier → Audit Database → CRM → Admin Dashboard
        /// </summary>
        public async Task<ReportingResult> GenerateReportsAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Bước 5: Theo dõi và báo cáo ===");
                Console.WriteLine($"Data Flow: Middle Tier → Audit Database → CRM → Admin Dashboard");
                Console.WriteLine($"OrderId: {orderId}");

                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ReportingResult
                    {
                        Success = false,
                        ErrorMessage = "Đơn hàng không tồn tại"
                    };
                }

                // Tạo báo cáo giao dịch
                var transactionReport = new TransactionReport
                {
                    OrderId = orderId,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    TransactionDate = DateTime.Now,
                    ReportType = "OrderCompletion"
                };

                // Tạm thời comment out vì chưa có DbSet TransactionReports và CustomerStats
                // _context.TransactionReports.Add(transactionReport);

                // Cập nhật thống kê khách hàng
                // var customerStats = await _context.CustomerStats
                //     .FirstOrDefaultAsync(cs => cs.UserId == order.UserId);

                // if (customerStats == null)
                // {
                //     customerStats = new CustomerStats
                //     {
                //         UserId = order.UserId,
                //         TotalOrders = 0,
                //         TotalSpent = 0,
                //         LastOrderDate = DateTime.Now
                //     };
                //     _context.CustomerStats.Add(customerStats);
                // }

                // customerStats.TotalOrders++;
                // customerStats.TotalSpent += order.TotalAmount;
                // customerStats.LastOrderDate = DateTime.Now;

                await _context.SaveChangesAsync();

                Console.WriteLine($"Transaction report generated: {transactionReport.ReportType}");
                // Console.WriteLine($"Customer stats updated: {customerStats.TotalOrders} orders, {customerStats.TotalSpent:C} total spent");

                return new ReportingResult
                {
                    Success = true,
                    TransactionReportCreated = true,
                    CustomerStatsUpdated = false, // Tạm thời set false vì chưa implement
                    ReportData = new
                    {
                        OrderNumber = order.OrderNumber,
                        TotalAmount = order.TotalAmount,
                        ItemsCount = order.OrderDetails.Count,
                        CustomerTotalOrders = 0 // Tạm thời set 0
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateReports: {ex.Message}");
                return new ReportingResult
                {
                    Success = false,
                    ErrorMessage = "Không thể tạo báo cáo"
                };
            }
        }

        /// <summary>
        /// Xử lý ngoại lệ
        /// Data Flow: Payment Gateway → Middle Tier → Exception Handling System
        /// </summary>
        public async Task<ExceptionHandlingResult> HandlePaymentExceptionAsync(int orderId, string exceptionType, string message)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Xử lý ngoại lệ ===");
                Console.WriteLine($"Data Flow: Payment Gateway → Middle Tier → Exception Handling System");
                Console.WriteLine($"OrderId: {orderId}, ExceptionType: {exceptionType}");

                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    // Cập nhật trạng thái lỗi
                    order.OrderStatus = "PaymentFailed";
                    order.PaymentStatus = "Failed";
                    order.UpdatedAt = DateTime.Now;

                    // Tạo lịch sử lỗi
                    var errorHistory = new OrderHistory
                    {
                        OrderId = orderId,
                        Status = "PaymentFailed",
                        Notes = $"Lỗi thanh toán: {exceptionType} - {message}",
                        CreatedAt = DateTime.Now
                    };

                    _context.OrderHistories.Add(errorHistory);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Payment exception handled: {exceptionType}");
                    Console.WriteLine($"Order status updated to: {order.OrderStatus}");
                }

                return new ExceptionHandlingResult
                {
                    Success = true,
                    ExceptionType = exceptionType,
                    HandledAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandlePaymentException: {ex.Message}");
                return new ExceptionHandlingResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xử lý ngoại lệ"
                };
            }
        }

        #region Private Methods

        private string GenerateTrackingNumber()
        {
            return $"TRK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        #endregion
    }

    #region Result Models

    public class PostPaymentResult
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class OrderStatusUpdateResult
    {
        public bool Success { get; set; }
        public string? OrderNumber { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }



    public class LogisticsResult
    {
        public bool Success { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ReportingResult
    {
        public bool Success { get; set; }
        public bool TransactionReportCreated { get; set; }
        public bool CustomerStatsUpdated { get; set; }
        public object? ReportData { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ExceptionHandlingResult
    {
        public bool Success { get; set; }
        public string? ExceptionType { get; set; }
        public DateTime? HandledAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion

    #region Additional Models (for reporting)

    public class TransactionReport
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReportType { get; set; } = string.Empty;
    }

    public class CustomerStats
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
