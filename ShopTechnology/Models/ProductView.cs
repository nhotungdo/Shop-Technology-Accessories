using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class ProductView
    {
        [Key]
        public int ProductViewId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? UserId { get; set; }

        [Required]
        public DateTime ViewDate { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? Referrer { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
