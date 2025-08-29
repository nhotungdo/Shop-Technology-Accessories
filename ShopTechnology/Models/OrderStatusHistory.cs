using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public OrderStatus OldStatus { get; set; }

        public OrderStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual ApplicationUser? ChangedByUser { get; set; }
    }
}
