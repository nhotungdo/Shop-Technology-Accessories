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
    }
}
