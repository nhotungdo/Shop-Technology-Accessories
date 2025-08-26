using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using AutoMapper;

namespace ShopTechnology.Controllers;

public class ProductController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        ShopTechnologyAccessoriesContext context,
        IProductService productService,
        ICategoryService categoryService,
        IReviewService reviewService,
        IMapper mapper,
        ILogger<ProductController> logger)
    {
        _context = context;
        _productService = productService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortBy, int page = 1)
    {
        try
        {
            const int pageSize = 12;
            var products = await _productService.GetProductsAsync(categoryId, searchTerm, null, null, sortBy, page, pageSize);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = products.TotalPages;
            ViewBag.TotalCount = products.TotalCount;

            return View(_mapper.Map<List<ProductViewModel>>(products.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products list");
            return View("Error");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to load product details for ID: {ProductId}", id);

            // Load product with all related data
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.User)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.ReviewImages)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Found product: {ProductName}", product.Name);

            // Increment view count
            product.ViewCount++;
            await _context.SaveChangesAsync();

            // Map to ProductViewModel
            var productViewModel = _mapper.Map<ProductViewModel>(product);

            // Get related products from same category
            var relatedProducts = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id && p.IsActive)
                .OrderByDescending(p => p.ViewCount)
                .Take(4)
                .ToListAsync();

            // Get reviews for this product
            var reviews = await _reviewService.GetReviewsByProductIdAsync(id);

            // Check if user is authenticated and get cart/wishlist status
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
                {
                    // Check if product is in user's cart
                    var cartItem = await _context.CartItems
                        .Include(ci => ci.Cart)
                        .FirstOrDefaultAsync(ci => ci.ProductId == id && ci.Cart.UserId == userIdInt);

                    if (cartItem != null)
                    {
                        productViewModel.IsInCart = true;
                        productViewModel.CartQuantity = cartItem.Quantity;
                    }

                    // Check if product is in user's wishlist
                    var wishlistItem = await _context.Wishlists
                        .FirstOrDefaultAsync(w => w.ProductId == id && w.UserId == userIdInt);

                    if (wishlistItem != null)
                    {
                        productViewModel.IsInWishlist = true;
                    }
                }
            }

            ViewBag.RelatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProducts);
            ViewBag.Reviews = reviews;

            return View(productViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details for ID: {ProductId}. Error: {ErrorMessage}", id, ex.Message);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin sản phẩm. Vui lòng thử lại sau.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q, int? categoryId, string? sortBy, int page = 1)
    {
        try
        {
            const int pageSize = 12;
            var products = await _productService.GetProductsAsync(categoryId, q, null, null, sortBy, page, pageSize);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = q;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = products.TotalPages;
            ViewBag.TotalCount = products.TotalCount;

            return View("Index", _mapper.Map<List<ProductViewModel>>(products.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Category(int id, string? sortBy, int page = 1)
    {
        try
        {
            const int pageSize = 12;
            var products = await _productService.GetProductsAsync(id, null, null, null, sortBy, page, pageSize);
            var category = await _context.Categories.FindAsync(id);
            var categories = await _categoryService.GetAllCategoriesAsync();

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = category;
            ViewBag.CategoryId = id;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = products.TotalPages;
            ViewBag.TotalCount = products.TotalCount;

            return View("Index", _mapper.Map<List<ProductViewModel>>(products.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products by category ID: {CategoryId}", id);
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        try
        {
            var productCount = await _context.Products.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();

            return Json(new
            {
                success = true,
                productCount = productCount,
                categoryCount = categoryCount,
                message = "Database connection successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return Json(new
            {
                success = false,
                error = ex.Message,
                message = "Database connection failed"
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> InitDatabase()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            var productCount = await _context.Products.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();

            return Json(new
            {
                success = true,
                databaseCreated = true,
                productCount = productCount,
                categoryCount = categoryCount,
                message = "Database initialized successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            return Json(new
            {
                success = false,
                error = ex.Message,
                message = "Database initialization failed"
            });
        }
    }
}
