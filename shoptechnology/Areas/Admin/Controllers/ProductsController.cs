using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;

namespace ShopTechnologyAccessories.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductsController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;
    public ProductsController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products.Include(p => p.Category).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model)
    {
        // Clear any existing model state errors
        ModelState.Clear();
        
        // Manual validation
        if (string.IsNullOrWhiteSpace(model.ProductName))
        {
            ModelState.AddModelError(nameof(Product.ProductName), "Tên sản phẩm không được để trống");
        }
        
        if (model.CategoryId <= 0)
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Vui lòng chọn danh mục");
        }
        else if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.CategoryId))
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Danh mục không tồn tại");
        }
        
        if (model.Price <= 0)
        {
            ModelState.AddModelError(nameof(Product.Price), "Giá phải lớn hơn 0");
        }
        
        if (model.StockQuantity < 0)
        {
            ModelState.AddModelError(nameof(Product.StockQuantity), "Số lượng tồn kho không hợp lệ");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }
        
        try
        {
            // Set default values
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = null;
            
            _db.Products.Add(model);
            await _db.SaveChangesAsync();
            
            TempData["ToastMessage"] = "Thêm sản phẩm thành công!";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            ModelState.AddModelError(string.Empty, $"Lỗi khi lưu sản phẩm: {errorMessage}");
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi không xác định: {ex.Message}");
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        ViewBag.Categories = await _db.Categories.ToListAsync();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product model)
    {
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.CategoryId))
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Vui lòng chọn danh mục hợp lệ");
        }
        if (model.Price < 0)
        {
            ModelState.AddModelError(nameof(Product.Price), "Giá không hợp lệ");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }
        try
        {
            model.UpdatedAt = DateTime.UtcNow;
            _db.Products.Update(model);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Cập nhật sản phẩm thành công";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, $"Không thể cập nhật sản phẩm. Lý do: {ex.InnerException?.Message ?? ex.Message}");
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["ToastMessage"] = "Xóa sản phẩm thành công";
            TempData["ToastType"] = "success";
        }
        return RedirectToAction(nameof(Index));
    }
}


