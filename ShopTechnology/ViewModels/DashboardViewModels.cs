using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    
    // Revenue statistics
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    
    // Order statistics
    public int TodayOrders { get; set; }
    public int WeekOrders { get; set; }
    public int MonthOrders { get; set; }
    
    // User statistics
    public int TodayUsers { get; set; }
    public int WeekUsers { get; set; }
    public int MonthUsers { get; set; }
    
    // Pending orders
    public int PendingOrders { get; set; }
    
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
    public List<LowStockProductViewModel> LowStockProducts { get; set; } = new();
    public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new();
}

public class RecentOrderViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LowStockProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
}

public class TopSellingProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal Price { get; set; }
}

public class ChartDataViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class OrderPaymentViewModel
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class OrderViewModel
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderDetailViewModel> OrderDetails { get; set; } = new();
    public OrderPaymentViewModel? Payment { get; set; }
}

public class OrderDetailViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}



public class ProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public bool IsInWishlist { get; set; }
    public bool IsInCart { get; set; }
    public int CartQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
