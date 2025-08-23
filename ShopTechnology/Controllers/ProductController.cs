using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using ShopTechnology.DTOs;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace ShopTechnology.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, ShopTechnologyAccessoriesContext context, IMapper mapper)
        {
            _productService = productService;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                List<Product> products;
                var categories = await _context.Categories.ToListAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    products = await _productService.SearchProductsAsync(searchTerm);
                }
                else if (categoryId.HasValue)
                {
                    products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
                }
                else
                {
                    var pagedResult = await _productService.GetProductsAsync(categoryId, searchTerm, minPrice, maxPrice);
                    products = pagedResult.Items;
                }

                var productViewModels = _mapper.Map<List<ProductViewModel>>(products);

                ViewBag.Categories = categories;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategoryId = categoryId;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;

                return View(productViewModels);
            }
            catch (Exception ex)
            {
                // Log the error
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var productDto = await _productService.GetProductByIdAsync(id);
                if (productDto == null)
                {
                    return NotFound();
                }

                var product = _mapper.Map<ProductViewModel>(productDto);

                // Get related products (same category)
                var relatedProductDtos = await _productService.GetProductsByCategoryAsync(product.CategoryId);
                var relatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProductDtos)
                    .Where(p => p.ProductId != id)
                    .Take(4)
                    .ToList();

                ViewBag.RelatedProducts = relatedProducts;
                return View(product);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _productService.SearchProductsAsync(searchTerm);
            var productViewModels = _mapper.Map<List<ProductViewModel>>(products);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("Index", productViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> FilterByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            var productViewModels = _mapper.Map<List<ProductViewModel>>(products);

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("Index", productViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            var pagedResult = await _productService.GetProductsAsync(null, null, minPrice, maxPrice);
            var products = _mapper.Map<List<ProductViewModel>>(pagedResult.Items);

            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("Index", products);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                return Json(products);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get products" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return Json(new { error = "Product not found" });
                }

                return Json(product);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get product details" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                var images = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .Select(pi => new { pi.ImageUrl, pi.IsMain })
                    .ToListAsync();

                return Json(images);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get product images" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Where(r => r.ProductId == productId)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        UserName = r.User != null ? r.User.FullName : "Anonymous"
                    })
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Json(reviews);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get product reviews" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStatistics()
        {
            try
            {
                var totalProducts = await _context.Products.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();
                var averagePrice = await _context.Products.AverageAsync(p => p.Price);
                var totalReviews = await _context.Reviews.CountAsync();

                return Json(new
                {
                    TotalProducts = totalProducts,
                    TotalCategories = totalCategories,
                    AveragePrice = Math.Round(averagePrice, 2),
                    TotalReviews = totalReviews
                });
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get product statistics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopRatedProducts(int count = 5)
        {
            try
            {
                var topProducts = await _context.Products
                    .Select(p => new
                    {
                        p.ProductId,
                        p.ProductName,
                        p.Price,
                        p.Description,
                        AverageRating = _context.Reviews
                            .Where(r => r.ProductId == p.ProductId)
                            .Any()
                            ? _context.Reviews
                                .Where(r => r.ProductId == p.ProductId)
                                .Average(r => r.Rating)
                            : 0,
                        ReviewCount = _context.Reviews
                            .Where(r => r.ProductId == p.ProductId)
                            .Count()
                    })
                    .Where(p => p.ReviewCount > 0)
                    .OrderByDescending(p => p.AverageRating)
                    .Take(count)
                    .ToListAsync();

                return Json(topProducts);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get top rated products" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNewestProducts(int count = 5)
        {
            try
            {
                var newestProducts = await _context.Products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.ProductName,
                        p.Price,
                        p.Description,
                        p.CreatedAt
                    })
                    .ToListAsync();

                return Json(newestProducts);
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to get newest products" });
            }
        }
    }
}
