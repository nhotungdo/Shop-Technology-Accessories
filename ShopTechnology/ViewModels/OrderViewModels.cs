using ShopTechnology.Models;

namespace ShopTechnology.ViewModels
{
    public class OrderFilterViewModel
    {
        public string? OrderNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    public class CreateOrderViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public string? Notes { get; set; }
        public string? PromotionCode { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ProductImage { get; set; }
    }

    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingCarrier { get; set; }
        public string? Notes { get; set; }
        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new List<OrderStatusHistoryViewModel>();
    }

    public class OrderStatusHistoryViewModel
    {
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string? Notes { get; set; }
        public string? ChangedByUser { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
