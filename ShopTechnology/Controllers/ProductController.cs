using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            var products = await _productService.GetProductsAsync(filter);
            var categories = await _categoryService.GetActiveCategoriesAsync();

            ViewBag.Categories = categories;
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(id);
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        public async Task<IActionResult> Category(int id, ProductFilterViewModel filter)
        {
            filter.CategoryId = id;
            var products = await _productService.GetProductsAsync(filter);
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Category = category;
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var filter = new ProductFilterViewModel { SearchTerm = searchTerm };
            var products = await _productService.GetProductsAsync(filter);
            
            ViewBag.SearchTerm = searchTerm;
            return View("Index", products);
        }
    }
}
