using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        public ProductsController(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }
            model.CreatedAt = DateTime.Now;
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo sản phẩm";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }
            var p = await _context.Products.FindAsync(model.ProductId);
            if (p == null) return NotFound();
            p.ProductName = model.ProductName;
            p.Description = model.Description;
            p.Price = model.Price;
            p.StockQuantity = model.StockQuantity;
            p.CategoryId = model.CategoryId;
            p.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật sản phẩm";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
