using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShopTechnology.DTOs;

public class CartDTO
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CartItemDTO> CartItems { get; set; } = new();

    // Computed properties
    public int TotalItems => CartItems.Sum(item => item.Quantity);
    public decimal TotalAmount => CartItems.Sum(item => item.SubTotal);
    public bool IsEmpty => !CartItems.Any();
}

public class CartItemDTO
{
    public int CartItemId { get; set; }
    public Guid CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal SubTotal => Price * Quantity;
    public bool IsInStock { get; set; }
    public int AvailableStock { get; set; }
}

public class AddToCartDTO
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
