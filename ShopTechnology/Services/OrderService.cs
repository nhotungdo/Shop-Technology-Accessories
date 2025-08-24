using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public OrderService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderHistories.OrderByDescending(oh => oh.CreatedAt))
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.Now;

            // Add order history
            var orderHistory = new OrderHistory
            {
                OrderId = orderId,
                Status = status,
                Notes = $"Trạng thái đơn hàng được cập nhật thành: {status}",
                CreatedAt = DateTime.Now
            };

            _context.OrderHistories.Add(orderHistory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.PaymentStatus = status;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.OrderStatus == "Completed" || o.OrderStatus == "Delivered")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => (o.OrderStatus == "Completed" || o.OrderStatus == "Delivered") &&
                           o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetOrderCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<int> GetOrderCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .CountAsync();
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Payments)
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            // Only allow cancellation for pending or processing orders
            if (order.OrderStatus == "Pending" || order.OrderStatus == "Processing")
            {
                order.OrderStatus = "Cancelled";
                order.UpdatedAt = DateTime.Now;

                // Add order history
                var orderHistory = new OrderHistory
                {
                    OrderId = orderId,
                    Status = "Cancelled",
                    Notes = "Đơn hàng đã được hủy",
                    CreatedAt = DateTime.Now
                };

                _context.OrderHistories.Add(orderHistory);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
