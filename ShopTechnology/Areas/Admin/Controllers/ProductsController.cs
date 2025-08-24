using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ShopTechnologyAccessoriesContext context,
        IProductService productService,
        ICategoryService categoryService,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortBy, int page = 1)
    {
        try
        {
            const int pageSize = 20;
            var products = await _productService.GetProductsAsync(categoryId, searchTerm, null, null, sortBy, page, pageSize);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = products.TotalPages;
            ViewBag.TotalCount = products.TotalCount;

            return View(products.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products list");
            return View("Error");
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create product form");
            return View("Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        try
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi tạo sản phẩm.");

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(product);
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
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit product form for ID: {ProductId}", id);
            return View("Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        try
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật sản phẩm.");

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(product);
        }
    }

    public async Task<IActionResult> Delete(int id)
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
            _logger.LogError(ex, "Error loading delete product form for ID: {ProductId}", id);
            return View("Error");
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStock(int productId, int newStock)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            product.StockQuantity = newStock;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật tồn kho thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product ID: {ProductId}", productId);
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var lowStockProducts = await _productService.GetLowStockProductsAsync(10);
            return View(lowStockProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading low stock products");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> OutOfStock()
    {
        try
        {
            var outOfStockProducts = await _productService.GetOutOfStockProductsAsync();
            return View(outOfStockProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading out of stock products");
            return View("Error");
        }
    }
}
