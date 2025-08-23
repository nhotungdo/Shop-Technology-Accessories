using ShopTechnology.Services;

namespace ShopTechnology.Services
{
    public interface IPaymentService
    {
        Task<ServiceResult> ProcessPaymentAsync(int paymentId, string paymentMethod);
        Task<ServiceResult> RefundPaymentAsync(int paymentId);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
    }
}
