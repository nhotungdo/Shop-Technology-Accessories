using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderHistories.OrderByDescending(oh => oh.CreatedAt))
                .ThenInclude(oh => oh.UpdatedByUser)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderHistories.OrderByDescending(oh => oh.CreatedAt))
                .ThenInclude(oh => oh.UpdatedByUser)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderHistories.OrderByDescending(oh => oh.CreatedAt))
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(OrderFilterViewModel filter, int page = 1, int pageSize = 20)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.OrderNumber))
            {
                query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));
            }

            if (!string.IsNullOrEmpty(filter.CustomerEmail))
            {
                query = query.Where(o => o.User.Email.Contains(filter.CustomerEmail));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(o => o.OrderStatus == filter.Status);
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = orders,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<Order> CreateOrderAsync(CreateOrderViewModel model)
        {
            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                UserId = int.Parse(model.UserId),
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingProvince = model.ShippingProvince,
                ShippingPostalCode = model.ShippingPostalCode,
                OrderNotes = model.Notes,
                CreatedAt = DateTime.UtcNow,
                OrderStatus = "Pending",
                PaymentStatus = "Pending"
            };

            // Calculate totals
            decimal subtotal = 0;
            foreach (var item in model.Items)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSKU = item.SKU,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    ProductImage = item.ProductImage
                };

                order.OrderDetails.Add(orderDetail);
                subtotal += item.TotalPrice;
            }

            order.SubTotal = subtotal;
            order.TaxAmount = subtotal * 0.1m; // 10% tax
            order.ShippingFee = subtotal > 200 ? 0 : 10; // Free shipping over $200
            order.DiscountAmount = 0; // Will be calculated if promotion is applied
            order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingFee - order.DiscountAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add initial status history
            await AddOrderHistoryAsync(order.OrderId, "Pending", "Order created");

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? notes = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.OrderStatus;
                order.OrderStatus = status;
                // order.UpdatedAt = DateTime.UtcNow;

                // Update specific dates based on status
                switch (status)
                {
                    case "Shipped":
                        order.ShippedDate = DateTime.UtcNow;
                        break;
                    case "Delivered":
                        order.DeliveredDate = DateTime.UtcNow;
                        break;
                    case "Cancelled":
                        // Handle cancelled status
                        break;
                }

                await _context.SaveChangesAsync();

                // Add status history
                await AddOrderHistoryAsync(orderId, status, notes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            return await UpdateOrderStatusAsync(orderId, "Cancelled", reason);
        }

        public async Task<bool> AddOrderHistoryAsync(int orderId, string status, string? notes = null)
        {
            try
            {
                var history = new OrderHistory
                {
                    OrderId = orderId,
                    Status = status,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedByUserId = 1 // TODO: Get current user ID
                };

                _context.OrderHistories.Add(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(int orderId)
        {
            return await _context.OrderHistories
                .Include(oh => oh.UpdatedByUser)
                .Where(oh => oh.OrderId == orderId)
                .OrderByDescending(oh => oh.CreatedAt)
                .ToListAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var prefix = "ORD";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var suffix = random.Next(1000, 9999).ToString();

            var orderNumber = $"{prefix}{date}{suffix}";

            // Ensure uniqueness
            while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
            {
                suffix = random.Next(1000, 9999).ToString();
                orderNumber = $"{prefix}{date}{suffix}";
            }

            return orderNumber;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                order.PaymentStatus = status;
                // order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<OrderSummaryViewModel> GetOrderSummaryAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return new OrderSummaryViewModel();

            return new OrderSummaryViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedAt,
                Status = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                CustomerName = order.User.FullName,
                CustomerEmail = order.User.Email,
                ShippingAddress = order.ShippingAddress,
                Items = order.OrderDetails.Select(od => new OrderItemViewModel
                {
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    SKU = od.ProductSKU,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.TotalPrice,
                    ProductImage = od.ProductImage
                }).ToList(),
                Subtotal = order.SubTotal,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                TrackingNumber = order.TrackingNumber,
                ShippingCarrier = order.ShippingMethod,
                Notes = order.OrderNotes,
                StatusHistory = order.OrderHistories.Select(oh => new OrderHistoryViewModel
                {
                    Status = oh.Status,
                    Notes = oh.Notes,
                    UpdatedByUser = oh.UpdatedByUser != null ? oh.UpdatedByUser.FullName : null,
                    CreatedAt = oh.CreatedAt
                }).ToList()
            };
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate &&
                           o.CreatedAt <= endDate &&
                           o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
        }

        private string FormatAddress(string address)
        {
            return address;
        }
    }
}
