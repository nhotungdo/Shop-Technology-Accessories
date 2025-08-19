using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class WishlistDTO
{
    public int WishlistId { get; set; }
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsInStock { get; set; }
    public int AvailableStock { get; set; }
}

public class AddToWishlistDTO
{
    [Required]
    public int ProductId { get; set; }
}

public class WishlistSummaryDTO
{
    public Guid UserId { get; set; }
    public int TotalItems { get; set; }
    public List<WishlistDTO> Items { get; set; } = new();
}
