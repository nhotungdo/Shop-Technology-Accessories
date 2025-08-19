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
            var productDto = await _productService.GetProductByIdAsync(id);
            if (productDto == null)
            {
                return NotFound();
            }

            var product = _mapper.Map<ProductViewModel>(productDto);

            // Lấy sản phẩm liên quan (cùng danh mục)
            var relatedProductDtos = await _productService.GetProductsByCategoryAsync(product.CategoryId);
            var relatedProducts = _mapper.Map<List<ProductViewModel>>(relatedProductDtos)
                .Where(p => p.ProductId != id).Take(4).ToList();

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
    }
}
