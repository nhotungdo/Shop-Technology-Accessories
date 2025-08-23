using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Diagnostics;

namespace ShopTechnology.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public HomeController(
        ILogger<HomeController> logger,
        ShopTechnologyAccessoriesContext context,
        IProductService productService,
        ICategoryService categoryService)
    {
        _logger = logger;
        _context = context;
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
            var categories = await _categoryService.GetAllCategoriesAsync();
            var latestProducts = await _productService.GetLatestProductsAsync(6);

            var viewModel = new HomeViewModel
            {
                FeaturedProducts = featuredProducts,
                Categories = categories,
                LatestProducts = latestProducts
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View("Error");
        }
    }

    public async Task<IActionResult> Products(int? categoryId, string? searchTerm,
        decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1)
    {
        try
        {
            const int pageSize = 12;

            var products = await _productService.GetProductsAsync(
                categoryId, searchTerm, minPrice, maxPrice, sortBy, page, pageSize);

            var categories = await _categoryService.GetAllCategoriesAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products.Items,
                Categories = categories,
                CurrentCategoryId = categoryId,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(products.TotalCount / (double)pageSize),
                TotalCount = products.TotalCount
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products page");
            return View("Error");
        }
    }

    public async Task<IActionResult> ProductDetail(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(id, 4);
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                Reviews = reviews
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product detail for ID: {ProductId}", id);
            return View("Error");
        }
    }

    public async Task<IActionResult> About()
    {
        return View();
    }

    public async Task<IActionResult> Contact()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Contact(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            // TODO: Implement contact form submission
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
            return RedirectToAction(nameof(Contact));
        }
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return RedirectToAction(nameof(Products));
        }

        const int pageSize = 12;
        var products = await _productService.GetProductsAsync(null, q, null, null, null, page, pageSize);

        var viewModel = new SearchViewModel
        {
            SearchTerm = q,
            Products = products.Items,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(products.TotalCount / (double)pageSize),
            TotalCount = products.TotalCount
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Category(int id, int page = 1)
    {
        const int pageSize = 12;
        var products = await _productService.GetProductsAsync(id, null, null, null, null, page, pageSize);
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        var viewModel = new CategoryViewModel
        {
            Category = category,
            Products = products.Items,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(products.TotalCount / (double)pageSize),
            TotalCount = products.TotalCount
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> TestDatabase()
    {
        try
        {
            var productCount = await _context.Products.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();

            return Json(new
            {
                success = true,
                message = "Database connection successful",
                productCount = productCount,
                categoryCount = categoryCount
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Database connection failed",
                error = ex.Message
            });
        }
    }
}
