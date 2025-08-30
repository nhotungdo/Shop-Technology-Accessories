using ShopTechnology.Models;

namespace ShopTechnology.ViewModels
{
    public class OrderFilterViewModel
    {
        public string? OrderNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    public class CreateOrderViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingProvince { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
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
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingCarrier { get; set; }
        public string? Notes { get; set; }
        public List<OrderHistoryViewModel> StatusHistory { get; set; } = new List<OrderHistoryViewModel>();
    }

    public class OrderHistoryViewModel
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? UpdatedByUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
