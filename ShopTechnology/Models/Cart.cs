using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(50)]
        public string? PromotionCode { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
