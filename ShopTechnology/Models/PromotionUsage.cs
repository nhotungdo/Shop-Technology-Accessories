using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class PromotionUsage
    {
        public int Id { get; set; }

        public int PromotionId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int OrderId { get; set; }

        public decimal DiscountAmount { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Promotion Promotion { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
