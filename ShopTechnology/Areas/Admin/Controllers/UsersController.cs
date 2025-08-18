using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        public UsersController(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(Guid id, int roleId)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();
            u.RoleId = roleId;
            u.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
