using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public UserController(
            ShopTechnologyAccessoriesContext context,
            IProductService productService,
            ICartService cartService)
        {
            _context = context;
            _productService = productService;
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Kiểm tra xem user đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin user cần thiết mà không load navigation properties
            var userInfo = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == int.Parse(userId))
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Avatar,
                    RoleName = u.Role.Name
                })
                .FirstOrDefaultAsync();

            if (userInfo == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra nếu là Admin thì chuyển đến Admin Dashboard
            if (userInfo.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Lấy dữ liệu cho User Dashboard
            var dashboardData = new UserDashboardViewModel
            {
                User = new UserSummaryDto
                {
                    UserId = userInfo.UserId,
                    FullName = userInfo.FullName,
                    Email = userInfo.Email,
                    Avatar = userInfo.Avatar,
                    RoleName = userInfo.RoleName
                },
                RecentOrders = await _context.Orders
                    .Where(o => o.UserId == userInfo.UserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new OrderSummaryDto
                    {
                        OrderId = o.OrderId,
                        OrderNumber = o.OrderNumber,
                        OrderStatus = o.OrderStatus,
                        PaymentStatus = o.PaymentStatus,
                        TotalAmount = o.TotalAmount,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync(),
                WishlistCount = await _context.Wishlists
                    .Where(w => w.UserId == userInfo.UserId)
                    .CountAsync(),
                CartItemCount = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Where(ci => ci.Cart.UserId == userInfo.UserId)
                    .CountAsync(),
                RecommendedProducts = await _context.Products
                    .Include(p => p.ProductImages)
                    .Where(p => p.IsFeatured && p.IsActive)
                    .OrderByDescending(p => p.ViewCount)
                    .Take(4)
                    .Select(p => new ProductSummaryDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.Price,
                        MainImage = p.MainImage,
                        ImageUrls = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
                    })
                    .ToListAsync(),
                TotalOrders = await _context.Orders
                    .Where(o => o.UserId == userInfo.UserId)
                    .CountAsync(),
                TotalSpent = await _context.Orders
                    .Where(o => o.UserId == userInfo.UserId && o.PaymentStatus == "Paid")
                    .SumAsync(o => o.TotalAmount)
            };

            return View(dashboardData);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(User user)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || int.Parse(userId) != user.UserId)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cập nhật thông tin cơ bản
            existingUser.FullName = user.FullName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Address = user.Address;
            existingUser.City = user.City;
            existingUser.Province = user.Province;
            existingUser.PostalCode = user.PostalCode;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderHistories)
                .Where(o => o.OrderId == id && o.UserId == int.Parse(userId))
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(w => w.Product.Category)
                .Where(w => w.UserId == int.Parse(userId))
                .ToListAsync();

            return View(wishlist);
        }
    }

    public class UserDashboardViewModel
    {
        public UserSummaryDto User { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; }
        public int WishlistCount { get; set; }
        public int CartItemCount { get; set; }
        public List<ProductSummaryDto> RecommendedProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public string RoleName { get; set; }
    }

    public class ProductSummaryDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? MainImage { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
