using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ReviewImage
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AltText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Review Review { get; set; } = null!;
    }
}
