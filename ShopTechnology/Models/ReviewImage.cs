using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ReviewImage
    {
        [Key]
        public int ReviewImageId { get; set; }

        public int ReviewId { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Review Review { get; set; }
    }
}
