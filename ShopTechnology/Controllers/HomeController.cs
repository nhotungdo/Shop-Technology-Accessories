using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using AutoMapper;

namespace ShopTechnology.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBannerService _bannerService;
        private readonly IMapper _mapper;

        public HomeController(
            ShopTechnologyAccessoriesContext context,
            IProductService productService,
            ICategoryService categoryService,
            IBannerService bannerService,
            IMapper mapper)
        {
            _context = context;
            _productService = productService;
            _categoryService = categoryService;
            _bannerService = bannerService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra thông báo lỗi từ query string
            var error = Request.Query["error"].ToString();
            if (error == "access_denied")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào trang quản trị. Chỉ Admin mới có thể truy cập.";
            }

            var viewModel = new HomeViewModel
            {
                // Hero Banner - Sản phẩm nổi bật
                HeroProducts = await _productService.GetFeaturedProductsAsync(3),

                // Categories - Danh mục sản phẩm
                Categories = await _categoryService.GetFeaturedCategoriesAsync(6),

                // Best Sellers - Sản phẩm bán chạy
                BestSellers = await _productService.GetHotProductsAsync(9),

                // New Arrivals - Sản phẩm mới về
                NewArrivals = await _productService.GetNewProductsAsync(9),

                // Promotions - Khuyến mãi và deal đặc biệt
                PromotionalProducts = await _productService.GetPromotionalProductsAsync(6),

                // Personalized Recommendations - Khuyến nghị cá nhân hóa
                RecommendedProducts = await GetPersonalizedRecommendationsAsync(),

                // Banners
                Banners = await _bannerService.GetActiveBannersAsync("Homepage")
            };

            return View(viewModel);
        }

        private async Task<List<Product>> GetPersonalizedRecommendationsAsync()
        {
            try
            {
                // Nếu user đã đăng nhập, lấy sản phẩm dựa trên lịch sử
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst("UserId")?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId) && userId > 0)
                    {
                        // Lấy sản phẩm dựa trên lịch sử mua hàng - sử dụng query đơn giản hơn
                        var userOrderDetails = await _context.OrderDetails
                            .Where(od => _context.Orders.Any(o => o.OrderId == od.OrderId && o.UserId == userId))
                            .Select(od => od.ProductId)
                            .Distinct()
                            .ToListAsync();

                        if (userOrderDetails.Any())
                        {
                            var recommendedProducts = await _context.Products
                                .Include(p => p.Category)
                                .Include(p => p.ProductImages)
                                .Where(p => p.IsActive && p.StockQuantity > 0)
                                .OrderByDescending(p => p.AverageRating)
                                .ThenByDescending(p => p.ViewCount)
                                .Take(8)
                                .ToListAsync();

                            if (recommendedProducts.Any())
                                return recommendedProducts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error và fallback
                Console.WriteLine($"Error in GetPersonalizedRecommendationsAsync: {ex.Message}");
            }

            // Fallback: Lấy sản phẩm có rating cao nhất
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.AverageRating)
                .ThenByDescending(p => p.ViewCount)
                .Take(8)
                .ToListAsync();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.Now;
                contact.Status = "New";

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ với chúng tôi. Chúng tôi sẽ phản hồi sớm nhất có thể!";
                return RedirectToAction(nameof(Contact));
            }

            return View(contact);
        }

        public async Task<IActionResult> FAQ()
        {
            var faqs = await _context.FAQs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Category)
                .ToListAsync();

            return View(faqs);
        }

        public async Task<IActionResult> Search(string q, string category, string brand,
            decimal? minPrice, decimal? maxPrice, string sortBy = "name", int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Search by keyword
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.Name.Contains(q) ||
                                        (p.Description != null && p.Description.Contains(q)) ||
                                        (p.Brand != null && p.Brand.Contains(q)) ||
                                        (p.SKU != null && p.SKU.Contains(q)));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Slug == category);
            }

            // Filter by brand
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand != null && p.Brand == brand);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sort
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderBy(p => p.Name)
            };

            // Pagination
            int pageSize = 12;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new SearchViewModel
            {
                Products = _mapper.Map<List<ProductViewModel>>(products),
                SearchTerm = q,
                Category = category,
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Categories = (await _categoryService.GetAllCategoriesAsync()).Select(c => c.Name).ToList(),
                Brands = await _productService.GetAllBrandsAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Category(string slug, string sortBy = "name", int page = 1)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => (p.CategoryId == category.CategoryId ||
                            p.Category.ParentCategoryId == category.CategoryId));

            // Sort
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                _ => query.OrderBy(p => p.Name)
            };

            // Pagination
            int pageSize = 12;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new CategoryViewModel
            {
                Category = category,
                Products = _mapper.Map<List<ProductViewModel>>(products),
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Product(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.User)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.ReviewImages)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
            {
                return NotFound();
            }

            // Increment view count
            product.ViewCount++;
            await _context.SaveChangesAsync();

            // Get related products
            var relatedProducts = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId &&
                           p.ProductId != product.ProductId)
                .OrderByDescending(p => p.ViewCount)
                .Take(4)
                .ToListAsync();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProducts)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Compare(int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            return View(products);
        }
    }
}
