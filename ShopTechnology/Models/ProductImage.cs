using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(255)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(100)]
        public string? AltText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsMain { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}
