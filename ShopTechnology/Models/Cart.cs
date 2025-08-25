using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // public DateTime? UpdatedAt { get; set; } // Commented out - column may not exist in database
        // public DateTime? ExpiresAt { get; set; } // Commented out - column may not exist in database

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
