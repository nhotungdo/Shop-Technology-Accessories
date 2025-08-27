using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ShopTechnology.Services
{
    /// <summary>
    /// Service xử lý luồng dữ liệu đặt hàng theo Data Flow Diagram (DFD)
    /// Level 1: Phân tích quy trình chính - Three-tier architecture
    /// </summary>
    public interface IOrderFlowService
    {
        // Level 1: Quy trình chính
        Task<ProductBrowseResult> BrowseProductsAsync(string? category, string? searchTerm, int page = 1);
        Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity);
        Task<CartOperationResult> UpdateCartAsync(int userId, int productId, int quantity);
        Task<CartOperationResult> RemoveFromCartAsync(int userId, int productId);
        Task<CheckoutValidationResult> ValidateCheckoutAsync(int userId, CheckoutViewModel checkoutData);
        Task<PaymentProcessingResult> ProcessPaymentAsync(int userId, PaymentViewModel paymentData);
        Task<OrderConfirmationResult> ConfirmOrderAsync(int userId, int orderId);

        // Level 2: Quy trình chi tiết
        Task<InventoryValidationResult> ValidateInventoryAsync(int productId, int quantity);
        Task<PriceCalculationResult> CalculateTotalAsync(int userId, string? promoCode = null);
        Task<PaymentGatewayResult> ProcessPaymentGatewayAsync(PaymentViewModel paymentData);
        Task<InventoryUpdateResult> UpdateInventoryAsync(int productId, int quantity);
        Task<NotificationResult> SendOrderConfirmationAsync(int userId, int orderId);
    }

    public class OrderFlowService : IOrderFlowService
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IEmailService _emailService;

        public OrderFlowService(
            ShopTechnologyAccessoriesContext context,
            ICartService cartService,
            IProductService productService,
            IEmailService emailService)
        {
            _context = context;
            _cartService = cartService;
            _productService = productService;
            _emailService = emailService;
        }

        /// <summary>
        /// Level 1 - Bước 1: Duyệt sản phẩm
        /// Data Flow: Product Database → Middle Tier → Client Tier → Customer
        /// </summary>
        public async Task<ProductBrowseResult> BrowseProductsAsync(string? category, string? searchTerm, int page = 1)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 1: Duyệt sản phẩm ===");
                Console.WriteLine($"Data Flow: Product Database → Middle Tier → Client Tier → Customer");
                Console.WriteLine($"Parameters: Category={category}, Search={searchTerm}, Page={page}");

                var products = await _productService.GetProductsAsync(null, searchTerm, null, null, null, page, 12);

                Console.WriteLine($"Products retrieved: {products.Items.Count} items");
                Console.WriteLine("Data Flow completed successfully");

                return new ProductBrowseResult
                {
                    Success = true,
                    Products = products.Items,
                    TotalCount = products.TotalCount,
                    CurrentPage = page
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BrowseProducts: {ex.Message}");
                return new ProductBrowseResult
                {
                    Success = false,
                    ErrorMessage = "Không thể tải danh sách sản phẩm"
                };
            }
        }

        /// <summary>
        /// Level 1 - Bước 2: Thêm vào giỏ hàng
        /// Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store
        /// </summary>
        public async Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 2: Thêm vào giỏ hàng ===");
                Console.WriteLine($"Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store");
                Console.WriteLine($"Parameters: UserId={userId}, ProductId={productId}, Quantity={quantity}");

                // Level 2: Kiểm tra tồn kho trước khi thêm
                var inventoryResult = await ValidateInventoryAsync(productId, quantity);
                if (!inventoryResult.IsAvailable)
                {
                    return new CartOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Sản phẩm không đủ tồn kho. Còn lại: {inventoryResult.AvailableQuantity}"
                    };
                }

                await _cartService.AddToCartAsync(userId, productId, quantity);

                Console.WriteLine($"Product {productId} added to cart for user {userId}");
                Console.WriteLine("Data Flow completed successfully");

                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã thêm sản phẩm vào giỏ hàng"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddToCart: {ex.Message}");
                return new CartOperationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể thêm sản phẩm vào giỏ hàng"
                };
            }
        }

        public async Task<CartOperationResult> UpdateCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Cập nhật giỏ hàng ===");
                Console.WriteLine($"Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store");
                Console.WriteLine($"Parameters: UserId={userId}, ProductId={productId}, Quantity={quantity}");

                // Level 2: Kiểm tra tồn kho
                var inventoryResult = await ValidateInventoryAsync(productId, quantity);
                if (!inventoryResult.IsAvailable)
                {
                    return new CartOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Sản phẩm không đủ tồn kho. Còn lại: {inventoryResult.AvailableQuantity}"
                    };
                }

                await _cartService.UpdateQuantityAsync(userId, productId, quantity);

                Console.WriteLine($"Cart updated for user {userId}");
                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã cập nhật giỏ hàng"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCart: {ex.Message}");
                return new CartOperationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể cập nhật giỏ hàng"
                };
            }
        }

        public async Task<CartOperationResult> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Xóa khỏi giỏ hàng ===");
                Console.WriteLine($"Data Flow: Customer → Client Tier → Middle Tier → Cart Data Store");
                Console.WriteLine($"Parameters: UserId={userId}, ProductId={productId}");

                await _cartService.RemoveFromCartAsync(userId, productId);

                Console.WriteLine($"Product {productId} removed from cart for user {userId}");
                return new CartOperationResult
                {
                    Success = true,
                    Message = "Đã xóa sản phẩm khỏi giỏ hàng"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFromCart: {ex.Message}");
                return new CartOperationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xóa sản phẩm khỏi giỏ hàng"
                };
            }
        }

        /// <summary>
        /// Level 1 - Bước 3: Checkout
        /// Data Flow: Customer → Client Tier → Middle Tier → Order Database
        /// </summary>
        public async Task<CheckoutValidationResult> ValidateCheckoutAsync(int userId, CheckoutViewModel checkoutData)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 3: Checkout ===");
                Console.WriteLine($"Data Flow: Customer → Client Tier → Middle Tier → Order Database");
                Console.WriteLine($"Parameters: UserId={userId}");

                // Level 2: Kiểm tra giỏ hàng hợp lệ
                var cart = await _cartService.GetCartAsync(userId);
                if (cart.Items.Count == 0)
                {
                    return new CheckoutValidationResult
                    {
                        Success = false,
                        ErrorMessage = "Giỏ hàng trống"
                    };
                }

                // Level 2: Kiểm tra tồn kho cho tất cả sản phẩm
                foreach (var item in cart.Items)
                {
                    var inventoryResult = await ValidateInventoryAsync(item.ProductId, item.Quantity);
                    if (!inventoryResult.IsAvailable)
                    {
                        return new CheckoutValidationResult
                        {
                            Success = false,
                            ErrorMessage = $"Sản phẩm {item.ProductName} không đủ tồn kho"
                        };
                    }
                }

                // Level 2: Tính tổng giá
                var priceResult = await CalculateTotalAsync(userId, checkoutData.PromoCode);

                Console.WriteLine($"Checkout validation completed for user {userId}");
                Console.WriteLine($"Total items: {cart.Items.Count}, Total amount: {priceResult.TotalAmount}");

                return new CheckoutValidationResult
                {
                    Success = true,
                    CartViewModel = cart,
                    TotalAmount = priceResult.TotalAmount,
                    DiscountAmount = priceResult.DiscountAmount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidateCheckout: {ex.Message}");
                return new CheckoutValidationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xác thực thông tin checkout"
                };
            }
        }

        /// <summary>
        /// Level 1 - Bước 4: Thanh toán
        /// Data Flow: Middle Tier → Payment Gateway → Middle Tier → Order Database
        /// </summary>
        public async Task<PaymentProcessingResult> ProcessPaymentAsync(int userId, PaymentViewModel paymentData)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 4: Thanh toán ===");
                Console.WriteLine($"Data Flow: Middle Tier → Payment Gateway → Middle Tier → Order Database");
                Console.WriteLine($"Parameters: UserId={userId}, PaymentMethod={paymentData.PaymentMethod}");

                // Level 2: Xử lý thanh toán qua Payment Gateway
                var gatewayResult = await ProcessPaymentGatewayAsync(paymentData);
                if (!gatewayResult.Success)
                {
                    return new PaymentProcessingResult
                    {
                        Success = false,
                        ErrorMessage = gatewayResult.ErrorMessage
                    };
                }

                // Tạo đơn hàng sau khi thanh toán thành công
                var order = await CreateOrderAsync(userId, paymentData);

                Console.WriteLine($"Payment processed successfully for user {userId}");
                Console.WriteLine($"Order created: {order.OrderId}");

                return new PaymentProcessingResult
                {
                    Success = true,
                    OrderId = order.OrderId,
                    TransactionId = gatewayResult.TransactionId,
                    Message = "Thanh toán thành công"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPayment: {ex.Message}");
                return new PaymentProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xử lý thanh toán"
                };
            }
        }

        /// <summary>
        /// Level 1 - Bước 5: Xác nhận đơn hàng
        /// Data Flow: Middle Tier → Customer & Warehouse
        /// </summary>
        public async Task<OrderConfirmationResult> ConfirmOrderAsync(int userId, int orderId)
        {
            try
            {
                Console.WriteLine("=== DFD Level 1 - Bước 5: Xác nhận đơn hàng ===");
                Console.WriteLine($"Data Flow: Middle Tier → Customer & Warehouse");
                Console.WriteLine($"Parameters: UserId={userId}, OrderId={orderId}");

                // Level 2: Cập nhật tồn kho
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    var orderDetails = await _context.OrderDetails
                        .Where(od => od.OrderId == orderId)
                        .ToListAsync();

                    foreach (var detail in orderDetails)
                    {
                        await UpdateInventoryAsync(detail.ProductId, detail.Quantity);
                    }
                }

                // Level 2: Gửi thông báo xác nhận
                var notificationResult = await SendOrderConfirmationAsync(userId, orderId);

                Console.WriteLine($"Order {orderId} confirmed for user {userId}");
                Console.WriteLine("Data Flow completed successfully");

                return new OrderConfirmationResult
                {
                    Success = true,
                    OrderId = orderId,
                    Message = "Đơn hàng đã được xác nhận"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmOrder: {ex.Message}");
                return new OrderConfirmationResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xác nhận đơn hàng"
                };
            }
        }

        #region Level 2: Quy trình chi tiết

        /// <summary>
        /// Level 2: Kiểm tra tồn kho
        /// </summary>
        public async Task<InventoryValidationResult> ValidateInventoryAsync(int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Kiểm tra tồn kho ===");
                Console.WriteLine($"ProductId: {productId}, Required Quantity: {quantity}");

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return new InventoryValidationResult
                    {
                        IsAvailable = false,
                        AvailableQuantity = 0,
                        ErrorMessage = "Sản phẩm không tồn tại"
                    };
                }

                var isAvailable = product.StockQuantity >= quantity;
                Console.WriteLine($"Product: {product.Name}, Stock: {product.StockQuantity}, Available: {isAvailable}");

                return new InventoryValidationResult
                {
                    IsAvailable = isAvailable,
                    AvailableQuantity = product.StockQuantity,
                    ProductName = product.Name
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidateInventory: {ex.Message}");
                return new InventoryValidationResult
                {
                    IsAvailable = false,
                    AvailableQuantity = 0,
                    ErrorMessage = "Không thể kiểm tra tồn kho"
                };
            }
        }

        /// <summary>
        /// Level 2: Tính tổng giá
        /// </summary>
        public async Task<PriceCalculationResult> CalculateTotalAsync(int userId, string? promoCode = null)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Tính tổng giá ===");
                Console.WriteLine($"UserId: {userId}, PromoCode: {promoCode}");

                var cart = await _cartService.GetCartAsync(userId);
                decimal subtotal = cart.Items.Sum(item => item.UnitPrice * item.Quantity);
                decimal discountAmount = 0;

                // Áp dụng mã giảm giá nếu có
                if (!string.IsNullOrEmpty(promoCode))
                {
                    var promotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.Code == promoCode && p.IsActive);

                    if (promotion != null)
                    {
                        discountAmount = subtotal * (promotion.DiscountValue / 100m);
                        Console.WriteLine($"Promotion applied: {promotion.Name}, Discount: {discountAmount}");
                    }
                }

                decimal totalAmount = subtotal - discountAmount;
                Console.WriteLine($"Subtotal: {subtotal}, Discount: {discountAmount}, Total: {totalAmount}");

                return new PriceCalculationResult
                {
                    Subtotal = subtotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    PromoCode = promoCode
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateTotal: {ex.Message}");
                return new PriceCalculationResult
                {
                    Subtotal = 0,
                    DiscountAmount = 0,
                    TotalAmount = 0,
                    ErrorMessage = "Không thể tính tổng giá"
                };
            }
        }

        /// <summary>
        /// Level 2: Xử lý Payment Gateway
        /// </summary>
        public async Task<PaymentGatewayResult> ProcessPaymentGatewayAsync(PaymentViewModel paymentData)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Xử lý Payment Gateway ===");
                Console.WriteLine($"Payment Method: {paymentData.PaymentMethod}");

                // Simulate payment gateway processing
                await Task.Delay(1000); // Simulate API call

                // For demo purposes, always return success
                // In real implementation, this would integrate with actual payment gateway
                var transactionId = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();

                Console.WriteLine($"Payment processed successfully. Transaction ID: {transactionId}");

                return new PaymentGatewayResult
                {
                    Success = true,
                    TransactionId = transactionId,
                    Message = "Thanh toán thành công"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPaymentGateway: {ex.Message}");
                return new PaymentGatewayResult
                {
                    Success = false,
                    ErrorMessage = "Không thể xử lý thanh toán"
                };
            }
        }

        /// <summary>
        /// Level 2: Cập nhật tồn kho
        /// </summary>
        public async Task<InventoryUpdateResult> UpdateInventoryAsync(int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Cập nhật tồn kho ===");
                Console.WriteLine($"ProductId: {productId}, Quantity to reduce: {quantity}");

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return new InventoryUpdateResult
                    {
                        Success = false,
                        ErrorMessage = "Sản phẩm không tồn tại"
                    };
                }

                product.StockQuantity -= quantity;
                await _context.SaveChangesAsync();

                Console.WriteLine($"Inventory updated. New stock: {product.StockQuantity}");

                return new InventoryUpdateResult
                {
                    Success = true,
                    NewStockQuantity = product.StockQuantity,
                    ProductName = product.Name
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateInventory: {ex.Message}");
                return new InventoryUpdateResult
                {
                    Success = false,
                    ErrorMessage = "Không thể cập nhật tồn kho"
                };
            }
        }

        /// <summary>
        /// Level 2: Gửi thông báo xác nhận
        /// </summary>
        public async Task<NotificationResult> SendOrderConfirmationAsync(int userId, int orderId)
        {
            try
            {
                Console.WriteLine($"=== DFD Level 2 - Gửi thông báo xác nhận ===");
                Console.WriteLine($"UserId: {userId}, OrderId: {orderId}");

                var user = await _context.Users.FindAsync(userId);
                var order = await _context.Orders.FindAsync(orderId);

                if (user != null && order != null)
                {
                    // Send email confirmation
                    await _emailService.SendOrderConfirmationEmailAsync(
                        user.Email,
                        user.FullName,
                        order.OrderNumber
                    );

                    Console.WriteLine($"Order confirmation sent to {user.Email}");
                }

                return new NotificationResult
                {
                    Success = true,
                    Message = "Thông báo xác nhận đã được gửi"
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

        #endregion

        #region Private Methods

        private async Task<Order> CreateOrderAsync(int userId, PaymentViewModel paymentData)
        {
            var cart = await _cartService.GetCartAsync(userId);
            var priceResult = await CalculateTotalAsync(userId, paymentData.PromoCode);

            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                CustomerName = paymentData.CustomerName,
                CustomerEmail = paymentData.CustomerEmail,
                CustomerPhone = paymentData.CustomerPhone,
                ShippingAddress = paymentData.ShippingAddress,
                TotalAmount = priceResult.TotalAmount,
                OrderStatus = "Pending",
                PaymentMethod = paymentData.PaymentMethod,
                PaymentStatus = "Paid",
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
                    TotalPrice = item.UnitPrice * item.Quantity,
                    ProductImage = item.ProductImage,
                    ProductBrand = item.ProductBrand
                };

                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // Clear cart after successful order
            await _cartService.ClearCartAsync(userId);

            return order;
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyy}-{DateTime.Now:MMdd}-{DateTime.Now:HHmmss}";
        }

        #endregion
    }

    #region Result Models

    public class ProductBrowseResult
    {
        public bool Success { get; set; }
        public List<Product> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CartOperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CheckoutValidationResult
    {
        public bool Success { get; set; }
        public CartViewModel? CartViewModel { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class OrderConfirmationResult
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class InventoryValidationResult
    {
        public bool IsAvailable { get; set; }
        public int AvailableQuantity { get; set; }
        public string? ProductName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PriceCalculationResult
    {
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PromoCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class InventoryUpdateResult
    {
        public bool Success { get; set; }
        public int NewStockQuantity { get; set; }
        public string? ProductName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NotificationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion
}
