using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class OrderService : IOrderService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ShopTechnologyAccessoriesContext context,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(CreateOrderViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create payment record
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                Method = model.PaymentMethod,
                Amount = model.TotalAmount,
                Status = "Pending",
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Create order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = model.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = model.TotalAmount,
                Status = "Pending",
                PaymentId = payment.PaymentId,
                ShippingAddress = model.ShippingAddress
            };

            _context.Orders.Add(order);

            // Create order details
            foreach (var item in model.OrderItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };

                _context.OrderDetails.Add(orderDetail);

                // Update product stock
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send confirmation email
            var user = await _context.Users.FindAsync(model.UserId);
            if (user != null)
            {
                await _emailService.SendOrderConfirmationEmailAsync(
                    user.Email, 
                    order.OrderId.ToString(), 
                    order.TotalAmount);
            }

            _logger.LogInformation("Order created successfully: {OrderId}", order.OrderId);
            return order;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order");
            throw;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return false;

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            // Send status update email
            if (order.User != null)
            {
                await _emailService.SendOrderStatusUpdateEmailAsync(
                    order.User.Email,
                    order.OrderId.ToString(),
                    newStatus);
            }

            _logger.LogInformation("Order status updated: {OrderId} -> {Status}", orderId, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return false;
        }
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.Status != "Pending")
                return false;

            // Restore product stock
            foreach (var orderDetail in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetail.ProductId);
                if (product != null)
                {
                    product.StockQuantity += orderDetail.Quantity;
                }
            }

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order cancelled: {OrderId}", orderId);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error cancelling order");
            return false;
        }
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(string status)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders.Where(o => o.Status == "Completed");

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        return await query.SumAsync(o => o.TotalAmount);
    }

    public async Task<int> GetOrderCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        return await query.CountAsync();
    }

    public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }
}

public class CreateOrderViewModel
{
    public Guid UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemViewModel> OrderItems { get; set; } = new();
}

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
