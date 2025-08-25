using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Banner
    {
        [Key]
        public int BannerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(255)]
        public string? LinkUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(50)]
        public string? Position { get; set; } // Homepage, Category, Product, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
