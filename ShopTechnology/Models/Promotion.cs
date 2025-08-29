using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public PromotionType Type { get; set; }

        public decimal Value { get; set; } // Percentage or fixed amount

        public decimal? MinimumOrderAmount { get; set; }

        public decimal? MaximumDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; } = 0;

        public int? MaxUsagePerUser { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFirstTimeOnly { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
        public virtual ICollection<PromotionUsage> Usages { get; set; } = new List<PromotionUsage>();
    }

    public enum PromotionType
    {
        Percentage,
        FixedAmount,
        FreeShipping
    }
}
