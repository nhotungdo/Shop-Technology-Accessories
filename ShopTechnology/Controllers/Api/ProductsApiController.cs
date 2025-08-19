using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
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
            var products = await _productService.GetAllProductsAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                             p.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
            }

            // Apply sorting
            products = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "asc" ? products.OrderBy(p => p.ProductName).ToList() : products.OrderByDescending(p => p.ProductName).ToList(),
                "price" => sortOrder.ToLower() == "asc" ? products.OrderBy(p => p.Price).ToList() : products.OrderByDescending(p => p.Price).ToList(),
                "stock" => sortOrder.ToLower() == "asc" ? products.OrderBy(p => p.StockQuantity).ToList() : products.OrderByDescending(p => p.StockQuantity).ToList(),
                "date" => sortOrder.ToLower() == "asc" ? products.OrderBy(p => p.CreatedAt).ToList() : products.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => products.OrderBy(p => p.ProductName).ToList()
            };

            return Ok(products);
        }
        catch (Exception ex)
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
            if (product == null)
            {
                return NotFound(new { error = "Product not found." });
            }

            return Ok(product);
        }
        catch (Exception ex)
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
        catch (Exception ex)
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
        catch (Exception ex)
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
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving featured products." });
        }
    }

    [HttpGet("newest")]
    public async Task<ActionResult<List<ProductDTO>>> GetNewestProducts()
    {
        try
        {
            var products = await _productService.GetNewestProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
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
            var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving products by price range." });
        }
    }
}
