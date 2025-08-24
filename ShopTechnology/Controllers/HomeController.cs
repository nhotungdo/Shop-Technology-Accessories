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
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = await _productService.GetFeaturedProductsAsync(8),
                LatestProducts = await _productService.GetNewProductsAsync(8),
                NewProducts = await _productService.GetNewProductsAsync(8),
                HotProducts = await _productService.GetHotProductsAsync(8),
                Categories = await _categoryService.GetFeaturedCategoriesAsync(6),
                Banners = await _bannerService.GetActiveBannersAsync("Homepage")
            };

            return View(viewModel);
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
                .Where(f => true /* f.IsActive - removed because column doesn't exist */)
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
                                        p.Description.Contains(q) ||
                                        p.Brand.Contains(q) ||
                                        p.SKU.Contains(q));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Slug == category);
            }

            // Filter by brand
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand == brand);
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
                .FirstOrDefaultAsync(c => c.Slug == slug && true /* c.IsActive - removed because column doesn't exist */);

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
