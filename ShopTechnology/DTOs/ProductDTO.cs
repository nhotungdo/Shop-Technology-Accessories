using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class ProductDTO
{
    public int ProductId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    public List<string> ImageUrls { get; set; } = new();
    public string MainImageUrl { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Additional properties for product details
    public bool IsInStock => StockQuantity > 0;
    public string StockStatus => IsInStock ? "Còn hàng" : "Hết hàng";
}

public class CreateProductDTO
{
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public List<string> ImageUrls { get; set; } = new();
}

public class UpdateProductDTO
{
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public List<string> ImageUrls { get; set; } = new();
}
