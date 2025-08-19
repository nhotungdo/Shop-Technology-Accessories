using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services;

public class OrderService : IOrderService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public OrderService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Legacy methods for backward compatibility
    public async Task<(bool ok, Guid orderId, string message)> CreateOrderFromCartAsync(Guid userId, string shippingAddress, string paymentMethod)
    {
        var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || cart.CartItems.Count == 0) return (false, Guid.Empty, "Giỏ hàng trống");

        var products = await _context.Products.Where(p => cart.CartItems.Select(ci => ci.ProductId).Contains(p.ProductId)).ToListAsync();
        foreach (var ci in cart.CartItems)
        {
            var product = products.First(p => p.ProductId == ci.ProductId);
            if (product.StockQuantity < ci.Quantity)
                return (false, Guid.Empty, $"Sản phẩm {product.ProductName} không đủ hàng");
        }

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Pending",
            ShippingAddress = shippingAddress,
            TotalAmount = 0
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        decimal total = 0;
        foreach (var ci in cart.CartItems)
        {
            var product = products.First(p => p.ProductId == ci.ProductId);
            var detail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = ci.Quantity,
                Price = product.Price
            };
            _context.OrderDetails.Add(detail);
            total += product.Price * ci.Quantity;
            product.StockQuantity -= ci.Quantity;
        }

        order.TotalAmount = total;
        await _context.SaveChangesAsync();

        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();

        // Create payment record (pending)
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            Method = paymentMethod,
            Amount = total,
            PaymentDate = DateTime.Now,
            Status = "Pending"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        order.PaymentId = payment.PaymentId;
        await _context.SaveChangesAsync();

        return (true, order.OrderId, "Đặt hàng thành công");
    }

    public async Task<List<OrderViewModel>> GetOrderHistoryAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var orderIds = orders.Select(o => o.OrderId).ToList();
        var details = await _context.OrderDetails
            .Where(d => orderIds.Contains(d.OrderId))
            .Include(d => d.Product)
            .ThenInclude(p => p.ProductImages)
            .ToListAsync();

        var payments = await _context.Payments.Where(p => p != null && orderIds.Contains(p.PaymentId)).ToListAsync();

        var result = new List<OrderViewModel>();
        foreach (var o in orders)
        {
            var od = details.Where(d => d.OrderId == o.OrderId).ToList();
            var pay = payments.FirstOrDefault(p => p.PaymentId == o.PaymentId);
            result.Add(new OrderViewModel
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentId = o.PaymentId,
                ShippingAddress = o.ShippingAddress,
                Payment = pay != null ? new PaymentViewModel
                {
                    PaymentId = pay.PaymentId,
                    Method = pay.Method,
                    Amount = pay.Amount,
                    PaymentDate = pay.PaymentDate,
                    Status = pay.Status
                } : null,
                OrderDetails = od.Select(d => new OrderDetailViewModel
                {
                    OrderDetailId = d.OrderDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.Product.ProductName,
                    ProductImage = d.Product.ProductImages?.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? string.Empty,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            });
        }

        return result;
    }

    public async Task<OrderViewModel?> GetOrderAsync(Guid orderId)
    {
        var o = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (o == null) return null;
        var details = await _context.OrderDetails.Where(d => d.OrderId == orderId)
            .Include(d => d.Product)
            .ThenInclude(p => p.ProductImages)
            .ToListAsync();
        var pay = o.PaymentId.HasValue ? await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == o.PaymentId.Value) : null;

        return new OrderViewModel
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            PaymentId = o.PaymentId,
            ShippingAddress = o.ShippingAddress,
            Payment = pay != null ? new PaymentViewModel
            {
                PaymentId = pay.PaymentId,
                Method = pay.Method,
                Amount = pay.Amount,
                PaymentDate = pay.PaymentDate,
                Status = pay.Status
            } : null,
            OrderDetails = details.Select(d => new OrderDetailViewModel
            {
                OrderDetailId = d.OrderDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product.ProductName,
                ProductImage = d.Product.ProductImages?.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? string.Empty,
                Quantity = d.Quantity,
                Price = d.Price
            }).ToList()
        };
    }

    // New methods for updated interface
    public async Task<List<OrderDTO>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        return _mapper.Map<OrderDTO>(order);
    }

    public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto)
    {
        var order = _mapper.Map<Order>(createOrderDto);
        order.OrderId = Guid.NewGuid();
        order.OrderDate = DateTime.UtcNow;
        order.Status = "Pending";

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.OrderId) ?? throw new InvalidOperationException("Failed to create order");
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<OrderDTO>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return false;
        }

        if (order.Status != "Pending" && order.Status != "Paid")
        {
            throw new InvalidOperationException("Order cannot be cancelled in current status");
        }

        // Restore stock
        foreach (var detail in order.OrderDetails)
        {
            var product = await _context.Products.FindAsync(detail.ProductId);
            if (product != null)
            {
                product.StockQuantity += detail.Quantity;
            }
        }

        order.Status = "Canceled";
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<OrderDTO>> GetRecentOrdersAsync(int count)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<List<OrderDTO>> GetOrdersByStatusAsync(string status)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<List<OrderDTO>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<List<OrderDTO>>(orders);
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Orders
            .Where(o => o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Orders
            .Where(o => o.Status == "Completed" && o.OrderDate >= startDate && o.OrderDate <= endDate)
            .SumAsync(o => o.TotalAmount);
    }
}
