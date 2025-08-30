using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = "Percentage";

        public decimal DiscountValue { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public decimal? MaximumDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPublic { get; set; } = true;

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    }
}
