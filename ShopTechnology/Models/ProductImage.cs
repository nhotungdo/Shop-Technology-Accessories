using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AltText { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsMain { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}
