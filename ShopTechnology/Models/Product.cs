using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? LongDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public decimal? CompareAtPrice { get; set; }

        public decimal? CostPrice { get; set; }

        public int StockQuantity { get; set; } = 0;

        public int? MinStockLevel { get; set; }

        public int? MaxStockLevel { get; set; }

        public bool TrackInventory { get; set; } = true;

        public bool AllowBackorder { get; set; } = false;

        public int CategoryId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Slug { get; set; }

        public decimal Weight { get; set; } = 0;

        [MaxLength(50)]
        public string? WeightUnit { get; set; } = "kg";

        public decimal Length { get; set; } = 0;
        public decimal Width { get; set; } = 0;
        public decimal Height { get; set; } = 0;

        [MaxLength(50)]
        public string? DimensionUnit { get; set; } = "cm";

        // Product flags
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsHot { get; set; } = false;
        public bool IsOnSale { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // SEO properties
        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        [MaxLength(200)]
        public string? MetaKeywords { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    }
}
