using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ProductPromotion
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int PromotionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Promotion Promotion { get; set; } = null!;
    }
}
