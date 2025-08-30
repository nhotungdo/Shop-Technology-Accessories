using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var dashboard = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),

                TodayRevenue = await GetTotalRevenueAsync(today, today),
                WeekRevenue = await GetTotalRevenueAsync(startOfWeek, today),
                MonthRevenue = await GetTotalRevenueAsync(startOfMonth, today),

                TodayOrders = await GetTotalOrdersAsync(today, today),
                WeekOrders = await GetTotalOrdersAsync(startOfWeek, today),
                MonthOrders = await GetTotalOrdersAsync(startOfMonth, today),

                TodayUsers = await GetTotalCustomersAsync(today, today),
                WeekUsers = await GetTotalCustomersAsync(startOfWeek, today),
                MonthUsers = await GetTotalCustomersAsync(startOfMonth, today),

                PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending"),

                RecentOrders = await GetRecentOrdersAsync(),
                LowStockProducts = await GetLowStockProductsAsync(),
                TopSellingProducts = await GetTopSellingProductsAsync()
            };

            return dashboard;
        }

        public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = await GetTotalRevenueAsync(startDate, endDate),
                TotalOrders = await GetTotalOrdersAsync(startDate, endDate),
                TotalCustomers = await GetTotalCustomersAsync(startDate, endDate),
                AverageOrderValue = await GetAverageOrderValueAsync(startDate, endDate),
                DailySales = (await GetDailySalesAsync(startDate, endDate)).ToList(),
                TopProducts = (await GetTopProductsAsync(10)).ToList(),
                TopCategories = (await GetTopCategoriesAsync(5)).ToList()
            };

            return report;
        }

        public async Task<IEnumerable<TopProductViewModel>> GetTopProductsAsync(int count = 10)
        {
            return await _context.OrderDetails
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.TotalPrice),
                    AverageRating = 0, // Would need to join with reviews
                    ReviewCount = 0 // Would need to join with reviews
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopCategoryViewModel>> GetTopCategoriesAsync(int count = 5)
        {
            return await _context.OrderDetails
                .Join(_context.Products, od => od.ProductId, p => p.ProductId, (od, p) => new { od, p })
                .Join(_context.Categories, x => x.p.CategoryId, c => c.CategoryId, (x, c) => new { x.od, x.p, c })
                .GroupBy(x => new { x.c.CategoryId, x.c.Name })
                .Select(g => new TopCategoryViewModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    ProductCount = g.Select(x => x.p.ProductId).Distinct().Count(),
                    QuantitySold = g.Sum(x => x.od.Quantity),
                    TotalRevenue = g.Sum(x => x.od.TotalPrice)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(count)
                .ToListAsync();
        }

        public async Task<CustomerAnalyticsViewModel> GetCustomerAnalyticsAsync()
        {
            var analytics = new CustomerAnalyticsViewModel
            {
                TotalCustomers = await _context.Users.CountAsync(),
                NewCustomers = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                ActiveCustomers = await _context.Orders
                    .Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync(),
                AverageCustomerValue = await _context.Orders
                    .GroupBy(o => o.UserId)
                    .Select(g => g.Sum(o => o.TotalAmount))
                    .DefaultIfEmpty()
                    .AverageAsync(),
                CustomerSegments = await GetCustomerSegmentsAsync()
            };

            return analytics;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate &&
                           o.CreatedAt <= endDate &&
                           o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
        }

        public async Task<int> GetTotalCustomersAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Users
                .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
        }

        public async Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .DefaultIfEmpty()
                .AverageAsync(o => o.TotalAmount);
        }

        public async Task<IEnumerable<DailySalesViewModel>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                    Customers = g.Select(o => o.UserId).Distinct().Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        private async Task<List<RecentOrderViewModel>> GetRecentOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    UserFullName = o.User.FullName,
                    TotalAmount = o.TotalAmount,
                    Status = o.OrderStatus,
                    StatusDisplay = o.OrderStatus,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<List<LowStockProductViewModel>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= 10) // Default minimum stock level
                .OrderBy(p => p.StockQuantity)
                .Take(10)
                .Select(p => new LowStockProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Unknown",
                    StockQuantity = p.StockQuantity,
                    Price = p.Price
                })
                .ToListAsync();
        }

        private async Task<List<TopSellingProductViewModel>> GetTopSellingProductsAsync()
        {
            return await _context.OrderDetails
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new TopSellingProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(od => od.Quantity),
                    Price = g.Average(od => od.UnitPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<CustomerSegmentViewModel>> GetCustomerSegmentsAsync()
        {
            var segments = new List<CustomerSegmentViewModel>();

            // High-value customers (>$1000 total spent)
            var highValueCount = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) > 1000)
                .CountAsync();

            var highValueTotal = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) > 1000)
                .SelectMany(g => g)
                .SumAsync(o => o.TotalAmount);

            segments.Add(new CustomerSegmentViewModel
            {
                Segment = "High Value",
                CustomerCount = highValueCount,
                TotalValue = highValueTotal,
                AverageValue = highValueCount > 0 ? highValueTotal / highValueCount : 0
            });

            // Medium-value customers ($100-$1000)
            var mediumValueCount = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) >= 100 && g.Sum(o => o.TotalAmount) <= 1000)
                .CountAsync();

            var mediumValueTotal = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) >= 100 && g.Sum(o => o.TotalAmount) <= 1000)
                .SelectMany(g => g)
                .SumAsync(o => o.TotalAmount);

            segments.Add(new CustomerSegmentViewModel
            {
                Segment = "Medium Value",
                CustomerCount = mediumValueCount,
                TotalValue = mediumValueTotal,
                AverageValue = mediumValueCount > 0 ? mediumValueTotal / mediumValueCount : 0
            });

            // Low-value customers (<$100)
            var lowValueCount = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) < 100)
                .CountAsync();

            var lowValueTotal = await _context.Orders
                .GroupBy(o => o.UserId)
                .Where(g => g.Sum(o => o.TotalAmount) < 100)
                .SelectMany(g => g)
                .SumAsync(o => o.TotalAmount);

            segments.Add(new CustomerSegmentViewModel
            {
                Segment = "Low Value",
                CustomerCount = lowValueCount,
                TotalValue = lowValueTotal,
                AverageValue = lowValueCount > 0 ? lowValueTotal / lowValueCount : 0
            });

            return segments;
        }
    }
}
