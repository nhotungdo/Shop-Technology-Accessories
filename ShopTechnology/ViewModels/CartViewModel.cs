using ShopTechnology.Models;

namespace ShopTechnology.ViewModels;

public class CartViewModel
{
    public List<CartItem> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? AppliedPromotionCode { get; set; }
    public string? AppliedPromotionName { get; set; }
}

public class CartItemViewModel
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ProductImage { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal TotalPrice => Price * Quantity;
}
