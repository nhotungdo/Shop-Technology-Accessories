using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(int orderId, string paymentMethod, decimal amount);
        Task<bool> ProcessPaymentAsync(string transactionId, string status, string? gatewayResponse = null);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetOrderPaymentsAsync(int orderId);
        Task<bool> RefundPaymentAsync(string transactionId, decimal amount, string reason);
        Task<string> GeneratePaymentUrlAsync(int orderId, string paymentMethod);
        Task<bool> ValidatePaymentCallbackAsync(string transactionId, string signature);
        Task<string> GetPaymentStatusAsync(string transactionId);
    }
}
