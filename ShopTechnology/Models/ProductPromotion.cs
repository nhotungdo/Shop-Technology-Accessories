using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ProductPromotion
    {
        [Key]
        public int ProductPromotionId { get; set; }

        public int ProductId { get; set; }

        public int PromotionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Promotion Promotion { get; set; }
    }
}
