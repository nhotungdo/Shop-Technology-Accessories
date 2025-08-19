using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading users.");
            return View(new List<UserDTO>());
        }
    }

    public IActionResult Create()
    {
        return View(new CreateUserDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDTO createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return View(createUserDto);
        }

        try
        {
            await _userService.CreateUserAsync(createUserDto);
            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(createUserDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while creating the user.");
            return View(createUserDto);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var updateUserDto = new UpdateUserDTO
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };

            return View(updateUserDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading the user.");
            return View(new UpdateUserDTO());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateUserDTO updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return View(updateUserDto);
        }

        try
        {
            await _userService.UpdateUserAsync(id, updateUserDto);
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(updateUserDto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating the user.");
            return View(updateUserDto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while deleting the user.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while loading the user.");
            return View(new UserDTO());
        }
    }
}
