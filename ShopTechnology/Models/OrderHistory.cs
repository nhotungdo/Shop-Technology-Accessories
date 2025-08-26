using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class OrderHistory
    {
        [Key]
        public int OrderHistoryId { get; set; }

        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public int? UpdatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual User? UpdatedByUser { get; set; }
    }
}
