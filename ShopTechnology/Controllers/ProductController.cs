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

            // First, check if we can connect to database
            var productCount = await _context.Products.CountAsync();
            _logger.LogInformation("Total products in database: {Count}", productCount);

            if (productCount == 0)
            {
                _logger.LogWarning("No products found in database");
                return View("Error");
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Found product: {ProductName}", product.Name);

            // Map to ProductViewModel
            var productViewModel = _mapper.Map<ProductViewModel>(product);

            // Get related products
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != id)
                .Take(4)
                .ToListAsync();

            // Get reviews for this product
            var reviews = await _reviewService.GetReviewsByProductIdAsync(id);

            ViewBag.RelatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProducts);
            ViewBag.Reviews = reviews;

            return View(productViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details for ID: {ProductId}. Error: {ErrorMessage}", id, ex.Message);
            return View("Error");
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
