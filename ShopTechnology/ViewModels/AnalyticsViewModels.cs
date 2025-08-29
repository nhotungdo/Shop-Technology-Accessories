namespace ShopTechnology.ViewModels
{
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

    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesViewModel> DailySales { get; set; } = new List<DailySalesViewModel>();
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
        public List<TopCategoryViewModel> TopCategories { get; set; } = new List<TopCategoryViewModel>();
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class TopCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CustomerAnalyticsViewModel
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public List<CustomerSegmentViewModel> CustomerSegments { get; set; } = new List<CustomerSegmentViewModel>();
    }

    public class CustomerSegmentViewModel
    {
        public string Segment { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
    }

    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int Customers { get; set; }
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
}
