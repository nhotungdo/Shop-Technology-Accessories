using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public PaymentService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> ProcessPaymentAsync(int paymentId, string paymentMethod)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return new ServiceResult { Success = false, Message = "Thanh toán không tồn tại." };
            }

            // Simulate payment processing
            // In a real application, you would integrate with actual payment gateways
            await Task.Delay(1000); // Simulate processing time

            // For demo purposes, we'll assume all payments are successful
            payment.Status = "Success";
            payment.UpdatedAt = DateTime.Now;
            payment.PaymentProvider = GetPaymentProvider(paymentMethod);

            // Update order payment status
            if (payment.Order != null)
            {
                payment.Order.PaymentStatus = "Paid";
                payment.Order.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Thanh toán thành công." };
        }

        public async Task<ServiceResult> RefundPaymentAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return new ServiceResult { Success = false, Message = "Thanh toán không tồn tại." };
            }

            if (payment.Status != "Success")
            {
                return new ServiceResult { Success = false, Message = "Chỉ có thể hoàn tiền cho thanh toán thành công." };
            }

            // Simulate refund processing
            await Task.Delay(1000);

            payment.Status = "Refunded";
            payment.UpdatedAt = DateTime.Now;

            // Update order payment status
            if (payment.Order != null)
            {
                payment.Order.PaymentStatus = "Refunded";
                payment.Order.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Hoàn tiền thành công." };
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        private string GetPaymentProvider(string paymentMethod)
        {
            return paymentMethod switch
            {
                "CreditCard" => "Stripe",
                "BankTransfer" => "Bank Transfer",
                "Momo" => "Momo",
                "ZaloPay" => "ZaloPay",
                "PayPal" => "PayPal",
                _ => "Unknown"
            };
        }
    }
}
