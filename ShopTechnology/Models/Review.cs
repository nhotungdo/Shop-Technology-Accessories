using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [StringLength(255)]
        public string? Title { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        public bool IsHelpful { get; set; } = false;

        public int HelpfulCount { get; set; } = 0;

        public int UnhelpfulCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();
    }
}
