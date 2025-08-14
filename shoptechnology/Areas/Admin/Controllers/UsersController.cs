using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;
using ShopTechnologyAccessories.Services;

namespace ShopTechnologyAccessories.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;
    private readonly PasswordHasher _hasher;
    public UsersController(ShopTechnologyAccessoriesContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.Include(u => u.Role).ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _db.Roles.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string fullName, string email, string password, int roleId)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin");
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View();
        }
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(string.Empty, "Email đã tồn tại");
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View();
        }
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = _hasher.HashPassword(password),
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();
        ViewBag.Roles = await _db.Roles.ToListAsync();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid userId, string fullName, string email, int roleId, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();
        user.FullName = fullName;
        user.Email = email;
        user.RoleId = roleId;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            user.PasswordHash = _hasher.HashPassword(newPassword);
        }
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLock(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        // Simplified lock: reuse UpdatedAt to indicate lock
        user.UpdatedAt = user.UpdatedAt == null ? DateTime.MaxValue : null;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}


