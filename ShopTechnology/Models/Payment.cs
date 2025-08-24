using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // CreditCard, BankTransfer, EWallet, etc.

        [StringLength(100)]
        public string? PaymentProvider { get; set; } // PayPal, Stripe, Momo, ZaloPay, etc.

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Refunded

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }

        [StringLength(255)]
        public string? PaymentUrl { get; set; }

        [StringLength(500)]
        public string? CallbackData { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
    }
}
