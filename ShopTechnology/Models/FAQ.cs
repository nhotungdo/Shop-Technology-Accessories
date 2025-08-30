using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Answer { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Category { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
