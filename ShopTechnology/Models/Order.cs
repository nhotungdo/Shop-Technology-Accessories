using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShippingCity { get; set; }

        [MaxLength(100)]
        public string? ShippingProvince { get; set; }

        [MaxLength(20)]
        public string? ShippingPostalCode { get; set; }

        [MaxLength(500)]
        public string? OrderNotes { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal ShippingFee { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; } = "Pending";

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(100)]
        public string? ShippingMethod { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
