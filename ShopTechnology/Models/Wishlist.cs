using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
