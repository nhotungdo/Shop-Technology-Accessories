using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        public OrdersController(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
