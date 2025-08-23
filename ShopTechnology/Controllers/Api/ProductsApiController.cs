using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;
using ShopTechnology.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ShopTechnology.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ShopTechnologyAccessoriesContext _context;

    public ProductsController(IProductService productService, ShopTechnologyAccessoriesContext context)
    {
        _productService = productService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDTO>>> GetProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        try
        {
            var result = await _productService.GetProductsAsync(
                categoryId: categoryId,
                searchTerm: searchTerm,
                minPrice: minPrice,
                maxPrice: maxPrice,
                sortBy: sortBy,
                page: 1,
                pageSize: 1000); // Large page size to get all products

            return Ok(result.Items);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving products." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product == null
                ? NotFound(new { error = "Product not found." })
                : Ok(product);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving the product." });
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<List<ProductDTO>>> GetProductsByCategory(int categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving products by category." });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductDTO>>> SearchProducts([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrEmpty(q))
            {
                return BadRequest(new { error = "Search term is required." });
            }

            var products = await _productService.SearchProductsAsync(q);
            return Ok(products);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while searching products." });
        }
    }

    [HttpGet("featured")]
    public async Task<ActionResult<List<ProductDTO>>> GetFeaturedProducts()
    {
        try
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(products);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving featured products." });
        }
    }

    [HttpGet("newest")]
    public async Task<ActionResult<List<ProductDTO>>> GetNewestProducts([FromQuery] int count = 10)
    {
        try
        {
            var products = await _productService.GetLatestProductsAsync(count);
            return Ok(products);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving newest products." });
        }
    }

    [HttpGet("price-range")]
    public async Task<ActionResult<List<ProductDTO>>> GetProductsByPriceRange(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice)
    {
        try
        {
            var result = await _productService.GetProductsAsync(
                minPrice: minPrice,
                maxPrice: maxPrice,
                page: 1,
                pageSize: 1000);
            return Ok(result.Items);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving products by price range." });
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetProductStatistics()
    {
        try
        {
            var result = await _productService.GetProductsAsync(page: 1, pageSize: 1);
            var totalProducts = result.TotalCount;
            var products = await _productService.GetProductsAsync(page: 1, pageSize: 1000);
            var averagePrice = products.Items.Any() ? products.Items.Average(p => p.Price) : 0;
            var totalCategories = await _context.Categories.CountAsync();

            return Ok(new
            {
                TotalProducts = totalProducts,
                AveragePrice = Math.Round(averagePrice, 2),
                TotalCategories = totalCategories
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving product statistics." });
        }
    }
}
