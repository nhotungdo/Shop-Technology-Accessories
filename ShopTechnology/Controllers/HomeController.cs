using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.Models;
using System.Linq;

namespace ShopTechnology.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBannerService _bannerService;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            IBannerService bannerService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _bannerService = bannerService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu từ database sử dụng Models
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
            var newProducts = await _productService.GetNewProductsAsync(8);
            var hotProducts = await _productService.GetHotProductsAsync(8);
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var banners = await _bannerService.GetActiveBannersAsync();

            // Tạo HomeViewModel sử dụng Models
            var viewModel = new HomeViewModel
            {
                // Sản phẩm nổi bật
                FeaturedProducts = featuredProducts.ToList(),

                // Sản phẩm mới
                NewProducts = newProducts.ToList(),

                // Sản phẩm hot
                HotProducts = hotProducts.ToList(),

                // Danh mục sản phẩm
                Categories = categories.ToList(),

                // Banners
                Banners = banners.ToList(),

                // Legacy properties cho backward compatibility
                HeroProducts = featuredProducts.Take(4).ToList(),
                BestSellers = hotProducts.Take(4).ToList(),
                NewArrivals = newProducts.Take(4).ToList(),
                PromotionalProducts = featuredProducts.Where(p => p.Price < 1000000).Take(4).ToList(),
                RecommendedProducts = featuredProducts.Take(4).ToList(),
                LatestProducts = newProducts.ToList()
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
