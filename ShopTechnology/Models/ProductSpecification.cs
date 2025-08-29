using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class ProductSpecification
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        public bool IsHighlighted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}
