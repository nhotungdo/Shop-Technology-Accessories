using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopTechnology.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ShopTechnologyAccessoriesContext _context;

        public ProductController(IProductService productService, ShopTechnologyAccessoriesContext context)
        {
            _productService = productService;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, decimal? minPrice, decimal? maxPrice)
        {
            List<ProductViewModel> products;
            List<Category> categories = await _context.Categories.ToListAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = await _productService.SearchProductsAsync(searchTerm);
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else if (minPrice.HasValue && maxPrice.HasValue)
            {
                products = await _productService.GetProductsByPriceRangeAsync(minPrice.Value, maxPrice.Value);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm liên quan (cùng danh mục)
            var relatedProducts = await _productService.GetProductsByCategoryAsync(product.CategoryId);
            relatedProducts = relatedProducts.Where(p => p.ProductId != id).Take(4).ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _productService.SearchProductsAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("Index", products);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            List<ProductViewModel> products;
            List<Category> categories = await _context.Categories.ToListAsync();

            if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else if (minPrice.HasValue && maxPrice.HasValue)
            {
                products = await _productService.GetProductsByPriceRangeAsync(minPrice.Value, maxPrice.Value);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View("Index", products);
        }
    }
}
