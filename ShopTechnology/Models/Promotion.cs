using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(20)]
        public string DiscountType { get; set; } = "Percentage"; // Percentage, FixedAmount

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsPublic { get; set; } = true;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
