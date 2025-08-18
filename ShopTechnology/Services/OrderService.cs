using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public OrderService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

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
    }
}
