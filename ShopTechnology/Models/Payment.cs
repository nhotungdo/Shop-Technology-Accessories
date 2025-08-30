using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PaymentProvider { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [MaxLength(255)]
        public string? PaymentUrl { get; set; }

        [MaxLength(500)]
        public string? CallbackData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}
