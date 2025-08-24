using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        [Required]
        [StringLength(200)]
        public string Question { get; set; }

        [Required]
        [StringLength(2000)]
        public string Answer { get; set; }

        [StringLength(50)]
        public string? Category { get; set; } // General, Shipping, Payment, etc.

        public int DisplayOrder { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
