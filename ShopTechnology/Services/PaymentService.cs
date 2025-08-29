using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> CreatePaymentAsync(int orderId, string paymentMethod, decimal amount)
        {
            var transactionId = GenerateTransactionId();
            var payment = new Payment
            {
                OrderId = orderId,
                TransactionId = transactionId,
                PaymentMethod = paymentMethod,
                Amount = amount,
                Status = PaymentStatus.Pending,
                Currency = "VND",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> ProcessPaymentAsync(string transactionId, PaymentStatus status, string? gatewayResponse = null)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
                if (payment == null) return false;

                payment.Status = status;
                payment.GatewayResponse = gatewayResponse;

                if (status == PaymentStatus.Paid)
                {
                    payment.ProcessedAt = DateTime.UtcNow;
                }
                else if (status == PaymentStatus.Failed)
                {
                    payment.FailedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Update order payment status
                var order = await _context.Orders.FindAsync(payment.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = status;
                    // order.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetOrderPaymentsAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> RefundPaymentAsync(string transactionId, decimal amount, string reason)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
                if (payment == null || payment.Status != PaymentStatus.Paid) return false;

                var refund = new Payment
                {
                    OrderId = payment.OrderId,
                    TransactionId = GenerateTransactionId(),
                    PaymentMethod = payment.PaymentMethod,
                    Amount = -amount, // Negative amount for refund
                    Status = PaymentStatus.Refunded,
                    Currency = payment.Currency,
                    Description = $"Refund: {reason}",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                _context.Payments.Add(refund);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePaymentUrlAsync(int orderId, string paymentMethod)
        {
            // This would integrate with actual payment gateways
            // For now, return a placeholder URL
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return string.Empty;

            return $"/payment/process?orderId={orderId}&method={paymentMethod}&amount={order.TotalAmount}";
        }

        public async Task<bool> ValidatePaymentCallbackAsync(string transactionId, string signature)
        {
            // This would validate the callback signature from payment gateway
            // For now, just check if payment exists
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
            return payment != null;
        }

        public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
            return payment?.Status ?? PaymentStatus.Pending;
        }

        private string GenerateTransactionId()
        {
            var prefix = "TXN";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random();
            var suffix = random.Next(1000, 9999).ToString();

            return $"{prefix}{timestamp}{suffix}";
        }
    }
}
