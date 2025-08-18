using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.ViewModels
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? PaymentId { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddress { get; set; } = string.Empty;

        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        public PaymentViewModel? Payment { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    public class PaymentViewModel
    {
        public Guid PaymentId { get; set; }
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
