using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string searchTerm, int? categoryId, string sortBy = "name", string sortOrder = "asc")
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                             p.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
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

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(products);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading products.");
            return View(new List<ProductDTO>());
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(new CreateProductDTO());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading categories.");
            return View(new CreateProductDTO());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDTO createProductDto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(createProductDto);
        }

        try
        {
            await _productService.CreateProductAsync(createProductDto);
            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(createProductDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while creating the product.");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(createProductDto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;

            var updateProductDto = new UpdateProductDTO
            {
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ImageUrls = product.ImageUrls
            };

            return View(updateProductDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading the product.");
            return View(new UpdateProductDTO());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDTO updateProductDto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(updateProductDto);
        }

        try
        {
            await _productService.UpdateProductAsync(id, updateProductDto);
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(updateProductDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating the product.");
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(updateProductDto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Product not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while deleting the product.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading the product.");
            return View(new ProductDTO());
        }
    }
}
