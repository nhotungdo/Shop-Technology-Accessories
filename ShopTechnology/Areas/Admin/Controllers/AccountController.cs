using Microsoft.AspNetCore.Mvc;
using ShopTechnology.ViewModels;
using ShopTechnology.Services;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Login()
    {
        // Redirect to main Account controller
        return RedirectToAction("Login", "Account", new { area = "" });
    }

    public IActionResult Logout()
    {
        // Redirect to main Account controller
        return RedirectToAction("Logout", "Account", new { area = "" });
    }

    public IActionResult Register()
    {
        // Redirect to main Account controller
        return RedirectToAction("Register", "Account", new { area = "" });
    }
}
