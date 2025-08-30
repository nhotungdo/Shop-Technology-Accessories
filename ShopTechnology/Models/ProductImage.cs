using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
