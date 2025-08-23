using System;
using System.Collections.Generic;

namespace ShopTechnology.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<LowStockProductViewModel> LowStockProducts { get; set; } = new();
    }

    public class RecentOrderViewModel
    {
        public Guid OrderId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
    }

    public class LowStockProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
    }

    public class ChartDataViewModel
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
