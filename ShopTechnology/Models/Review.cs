using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int Rating { get; set; } // 1-5 stars

        [Required]
        [MaxLength(1000)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false; // Verified purchase

        public bool IsApproved { get; set; } = false;

        public bool IsHelpful { get; set; } = false;

        public int HelpfulCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
    }
}
