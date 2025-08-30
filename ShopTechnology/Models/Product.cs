using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(50)]
        public string? SKU { get; set; }

        public int StockQuantity { get; set; } = 0;

        [Required]
        public int CategoryId { get; set; }

        [MaxLength(255)]
        public string? MainImage { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public bool IsNew { get; set; } = false;

        public bool IsHot { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public int SoldCount { get; set; } = 0;

        public decimal? AverageRating { get; set; }

        public int ReviewCount { get; set; } = 0;

        [MaxLength(100)]
        public string? Slug { get; set; }

        [MaxLength(255)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        [MaxLength(500)]
        public string? Keywords { get; set; }

        [MaxLength(100)]
        public string? Color { get; set; }

        [MaxLength(100)]
        public string? Material { get; set; }

        [MaxLength(100)]
        public string? Weight { get; set; }

        [MaxLength(100)]
        public string? Dimensions { get; set; }

        [MaxLength(500)]
        public string? Compatibility { get; set; }

        [MaxLength(100)]
        public string? Warranty { get; set; }

        [MaxLength(500)]
        public string? Features { get; set; }

        [MaxLength(500)]
        public string? PackageContents { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    }
}
