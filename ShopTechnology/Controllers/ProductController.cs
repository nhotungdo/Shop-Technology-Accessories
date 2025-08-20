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
            List<ProductDTO> productDtos;
            List<Category> categories = await _context.Categories.ToListAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productDtos = await _productService.SearchProductsAsync(searchTerm);
            }
            else if (categoryId.HasValue)
            {
                productDtos = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else if (minPrice.HasValue && maxPrice.HasValue)
            {
                productDtos = await _productService.GetProductsByPriceRangeAsync(minPrice.Value, maxPrice.Value);
            }
            else
            {
                productDtos = await _productService.GetAllProductsAsync();
            }

            // Convert DTOs to ViewModels
            var products = _mapper.Map<List<ProductViewModel>>(productDtos);

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                Console.WriteLine($"Product Details requested for ID: {id}");

                var productDto = await _productService.GetProductByIdAsync(id);
                if (productDto == null)
                {
                    Console.WriteLine($"Product not found for ID: {id}");
                    return NotFound();
                }

                Console.WriteLine($"Product found: {productDto.ProductName}, Category: {productDto.CategoryName}");

                var product = _mapper.Map<ProductViewModel>(productDto);
                Console.WriteLine($"Mapped to ViewModel - Category: {product.CategoryName}");

                // Lấy sản phẩm liên quan (cùng danh mục)
                var relatedProductDtos = await _productService.GetProductsByCategoryAsync(product.CategoryId);
                var relatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProductDtos)
                    .Where(p => p.ProductId != id).Take(4).ToList();

                Console.WriteLine($"Found {relatedProducts.Count} related products");

                ViewBag.RelatedProducts = relatedProducts;

                return View(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Details action: {ex.Message}");
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

            var productDtos = await _productService.SearchProductsAsync(searchTerm);
            var products = _mapper.Map<List<ProductViewModel>>(productDtos);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("Index", products);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            List<ProductDTO> productDtos;
            List<Category> categories = await _context.Categories.ToListAsync();

            if (categoryId.HasValue)
            {
                productDtos = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else if (minPrice.HasValue && maxPrice.HasValue)
            {
                productDtos = await _productService.GetProductsByPriceRangeAsync(minPrice.Value, maxPrice.Value);
            }
            else
            {
                productDtos = await _productService.GetAllProductsAsync();
            }

            var products = _mapper.Map<List<ProductViewModel>>(productDtos);

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View("Index", products);
        }

        [HttpGet]
        public async Task<IActionResult> TestProduct(int id)
        {
            try
            {
                var productDto = await _productService.GetProductByIdAsync(id);
                if (productDto != null)
                {
                    var productInfo = new
                    {
                        ProductId = productDto.ProductId,
                        ProductName = productDto.ProductName,
                        Description = productDto.Description,
                        Price = productDto.Price,
                        StockQuantity = productDto.StockQuantity,
                        CategoryId = productDto.CategoryId,
                        CategoryName = productDto.CategoryName,
                        ImageUrls = productDto.ImageUrls,
                        MainImageUrl = productDto.MainImageUrl
                    };

                    return Json(productInfo);
                }
                else
                {
                    return Json(new { error = "Product not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FixProductCategories()
        {
            try
            {
                // Sửa danh mục cho các sản phẩm
                var products = await _context.Products.ToListAsync();
                var categories = await _context.Categories.ToListAsync();

                int updatedCount = 0;

                foreach (var product in products)
                {
                    int? newCategoryId = null;

                    // Xác định danh mục dựa trên tên sản phẩm
                    if (product.ProductName.Contains("tai nghe", StringComparison.OrdinalIgnoreCase) ||
                        product.ProductName.Contains("headphone", StringComparison.OrdinalIgnoreCase))
                    {
                        newCategoryId = categories.FirstOrDefault(c => c.CategoryName.Contains("Tai nghe"))?.CategoryId;
                    }
                    else if (product.ProductName.Contains("bàn phím", StringComparison.OrdinalIgnoreCase) ||
                             product.ProductName.Contains("keyboard", StringComparison.OrdinalIgnoreCase))
                    {
                        newCategoryId = categories.FirstOrDefault(c => c.CategoryName.Contains("Bàn phím"))?.CategoryId;
                    }
                    else if (product.ProductName.Contains("sạc", StringComparison.OrdinalIgnoreCase) ||
                             product.ProductName.Contains("charger", StringComparison.OrdinalIgnoreCase))
                    {
                        newCategoryId = categories.FirstOrDefault(c => c.CategoryName.Contains("Sạc"))?.CategoryId;
                    }
                    else if (product.ProductName.Contains("ốp lưng", StringComparison.OrdinalIgnoreCase) ||
                             product.ProductName.Contains("case", StringComparison.OrdinalIgnoreCase))
                    {
                        newCategoryId = categories.FirstOrDefault(c => c.CategoryName.Contains("Phụ kiện"))?.CategoryId;
                    }
                    else if (product.ProductName.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
                             product.ProductName.Contains("ổ cứng", StringComparison.OrdinalIgnoreCase))
                    {
                        newCategoryId = categories.FirstOrDefault(c => c.CategoryName.Contains("Phụ kiện"))?.CategoryId;
                    }

                    if (newCategoryId.HasValue && newCategoryId.Value != product.CategoryId)
                    {
                        product.CategoryId = newCategoryId.Value;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã cập nhật {updatedCount} sản phẩm",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
