using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string? Currency { get; set; } = "VND";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? GatewayResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public DateTime? FailedAt { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // Navigation property
        public virtual Order Order { get; set; } = null!;
    }
}
