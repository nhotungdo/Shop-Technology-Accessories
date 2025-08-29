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
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory.OrderByDescending(sh => sh.ChangedAt))
                .ThenInclude(sh => sh.ChangedByUser)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory.OrderByDescending(sh => sh.ChangedAt))
                .ThenInclude(sh => sh.ChangedByUser)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory.OrderByDescending(sh => sh.ChangedAt))
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(OrderFilterViewModel filter, int page = 1, int pageSize = 20)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
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

            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            if (filter.PaymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= filter.EndDate.Value);
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
                .OrderByDescending(o => o.OrderDate)
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
                UserId = model.UserId,
                ShippingAddressId = model.ShippingAddressId,
                BillingAddressId = model.BillingAddressId,
                Notes = model.Notes,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending
            };

            // Calculate totals
            decimal subtotal = 0;
            foreach (var item in model.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    SKU = item.SKU,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    ProductImage = item.ProductImage,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
                subtotal += item.TotalPrice;
            }

            order.Subtotal = subtotal;
            order.TaxAmount = subtotal * 0.1m; // 10% tax
            order.ShippingAmount = subtotal > 200 ? 0 : 10; // Free shipping over $200
            order.DiscountAmount = 0; // Will be calculated if promotion is applied
            order.TotalAmount = order.Subtotal + order.TaxAmount + order.ShippingAmount - order.DiscountAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add initial status history
            await AddOrderStatusHistoryAsync(order.Id, OrderStatus.Pending, OrderStatus.Pending, "Order created");

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, string? notes = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.Status;
                order.Status = status;
                // order.UpdatedAt = DateTime.UtcNow;

                // Update specific dates based on status
                switch (status)
                {
                    case OrderStatus.Shipped:
                        order.ShippedDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Delivered:
                        order.DeliveredDate = DateTime.UtcNow;
                        break;
                    case OrderStatus.Cancelled:
                        order.CancelledDate = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();

                // Add status history
                await AddOrderStatusHistoryAsync(orderId, oldStatus, status, notes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, reason);
        }

        public async Task<bool> AddOrderStatusHistoryAsync(int orderId, OrderStatus oldStatus, OrderStatus newStatus, string? notes = null)
        {
            try
            {
                var history = new OrderStatusHistory
                {
                    OrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Notes = notes,
                    ChangedAt = DateTime.UtcNow
                };

                _context.OrderStatusHistories.Add(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId)
        {
            return await _context.OrderStatusHistories
                .Include(sh => sh.ChangedByUser)
                .Where(sh => sh.OrderId == orderId)
                .OrderByDescending(sh => sh.ChangedAt)
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

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
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
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CustomerName = $"{order.User.FirstName} {order.User.LastName}",
                CustomerEmail = order.User.Email,
                ShippingAddress = FormatAddress(order.ShippingAddress),
                BillingAddress = FormatAddress(order.BillingAddress),
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    SKU = oi.SKU,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    ProductImage = oi.ProductImage
                }).ToList(),
                Subtotal = order.Subtotal,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingAmount,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                TrackingNumber = order.TrackingNumber,
                ShippingCarrier = order.ShippingCarrier,
                Notes = order.Notes,
                StatusHistory = order.StatusHistory.Select(sh => new OrderStatusHistoryViewModel
                {
                    OldStatus = sh.OldStatus,
                    NewStatus = sh.NewStatus,
                    Notes = sh.Notes,
                    ChangedByUser = sh.ChangedByUser?.UserName,
                    ChangedAt = sh.ChangedAt
                }).ToList()
            };
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate &&
                           o.OrderDate <= endDate &&
                           o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
        }

        private string FormatAddress(Address address)
        {
            return $"{address.StreetAddress}, {address.City}, {address.State} {address.PostalCode}, {address.Country}";
        }
    }
}
