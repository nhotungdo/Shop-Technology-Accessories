using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;
using System.Security.Claims;

namespace ShopTechnologyAccessories.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _db;

    public CartController(ShopTechnologyAccessoriesContext db)
    {
        _db = db;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _db.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { CartId = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        cart ??= new Cart { CartItems = new List<CartItem>() };
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var userId = GetUserId();
        var cart = await GetOrCreateCartAsync(userId);
        var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
        if (item == null)
        {
            item = new CartItem { CartId = cart.CartId, ProductId = productId, Quantity = Math.Max(1, quantity) };
            _db.CartItems.Add(item);
        }
        else
        {
            item.Quantity += Math.Max(1, quantity);
        }
        await _db.SaveChangesAsync();
        TempData["success"] = "Sản phẩm đã được thêm vào giỏ hàng";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var item = await _db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            item.Quantity = Math.Max(1, quantity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var item = await _db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}


