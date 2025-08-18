using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

namespace ShopTechnology.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            var newestProducts = await _productService.GetNewestProductsAsync();

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.NewestProducts = newestProducts;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
