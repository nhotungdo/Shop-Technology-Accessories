using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;

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
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            var newProducts = await _productService.GetNewProductsAsync();
            var hotProducts = await _productService.GetHotProductsAsync();
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var banners = await _bannerService.GetActiveBannersAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedProducts = featuredProducts.Select(p => new FeaturedProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CompareAtPrice = p.CompareAtPrice,
                    ImageUrl = p.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl ?? string.Empty,
                    Slug = p.Slug,
                    AverageRating = (decimal)p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count(r => r.IsApproved),
                    CategoryName = p.Category?.Name ?? string.Empty,
                    IsNew = p.IsNew,
                    IsHot = p.IsHot,
                    IsOnSale = p.IsOnSale
                }).ToList(),
                NewProducts = newProducts.Select(p => new NewProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CompareAtPrice = p.CompareAtPrice,
                    ImageUrl = p.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl ?? string.Empty,
                    Slug = p.Slug,
                    AverageRating = (decimal)p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count(r => r.IsApproved),
                    CategoryName = p.Category?.Name ?? string.Empty,
                    IsNew = p.IsNew,
                    IsHot = p.IsHot,
                    IsOnSale = p.IsOnSale
                }).ToList(),
                HotProducts = hotProducts.Select(p => new HotProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CompareAtPrice = p.CompareAtPrice,
                    ImageUrl = p.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl ?? string.Empty,
                    Slug = p.Slug,
                    AverageRating = (decimal)p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count(r => r.IsApproved),
                    CategoryName = p.Category?.Name ?? string.Empty,
                    IsNew = p.IsNew,
                    IsHot = p.IsHot,
                    IsOnSale = p.IsOnSale
                }).ToList(),
                Categories = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Slug = c.Slug,
                    ProductCount = c.Products?.Count ?? 0,
                    Children = c.Children?.Select(child => new CategoryViewModel
                    {
                        Id = child.Id,
                        Name = child.Name,
                        Description = child.Description,
                        ImageUrl = child.ImageUrl,
                        Slug = child.Slug,
                        ProductCount = child.Products?.Count ?? 0
                    }).ToList() ?? new List<CategoryViewModel>()
                }).ToList(),
                Banners = banners.Select(b => new BannerViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    ImageUrl = b.ImageUrl,
                    LinkUrl = b.LinkUrl,
                    ButtonText = b.ButtonText,
                    DisplayOrder = b.DisplayOrder
                }).ToList()
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
