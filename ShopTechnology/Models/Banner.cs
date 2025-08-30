using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Banner
    {
        [Key]
        public int BannerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? LinkUrl { get; set; }

        [MaxLength(50)]
        public string? Position { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
