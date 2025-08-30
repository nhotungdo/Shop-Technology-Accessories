using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class ReviewImage
    {
        [Key]
        public int ReviewImageId { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; } = null!;
    }
}
