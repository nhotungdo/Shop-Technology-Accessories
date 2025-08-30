using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class ProductPromotion
    {
        [Key]
        public int ProductPromotionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int PromotionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("PromotionId")]
        public virtual Promotion Promotion { get; set; } = null!;
    }
}
