using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Services;
using ShopTechnology.DTOs;
using System.Security.Claims;

namespace ShopTechnology.Controllers;

public class AccountController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IUserService _userService;
    private readonly ILoginService _loginService;

    public AccountController(ShopTechnologyAccessoriesContext context, IUserService userService, ILoginService loginService)
    {
        _context = context;
        _userService = userService;
        _loginService = loginService;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var loginResult = await _loginService.ValidateUserAsync(model.Email, model.Password);

            if (loginResult.IsValid && loginResult.UserId.HasValue)
            {
                await _loginService.LogLoginAttemptAsync(model.Email, true, ipAddress, userAgent);

                HttpContext.Session.SetString("UserId", loginResult.UserId.Value.ToString());
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserName", loginResult.FullName ?? "User");
                HttpContext.Session.SetString("UserRole", loginResult.RoleName ?? "User");

                return loginResult.RoleName?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true
                    ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                    : RedirectToAction("Index", "Home");
            }

            await _loginService.LogLoginAttemptAsync(model.Email, false, ipAddress, userAgent);
            ModelState.AddModelError("", loginResult.ErrorMessage ?? "Email hoặc mật khẩu không đúng");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Có lỗi xảy ra khi đăng nhập: " + ex.Message);
        }

        return View(model);
    }

    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            if (await _userService.IsEmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng");
                return View(model);
            }

            var createUserDto = new CreateUserDTO
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                RoleId = 2
            };

            await _userService.CreateUserAsync(createUserDto);
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký: " + ex.Message);
        }

        return View(model);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _userService.ForgotPasswordAsync(model.Email);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Đã gửi email hướng dẫn đặt lại mật khẩu. Vui lòng kiểm tra hộp thư của bạn."
                : "Email không tồn tại trong hệ thống.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return View(model);
    }

    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Link đặt lại mật khẩu không hợp lệ.";
            return RedirectToAction(nameof(Login));
        }

        var isValid = await _userService.ValidateResetTokenAsync(email, token);
        if (!isValid)
        {
            TempData["ErrorMessage"] = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ.";
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordDTO { Token = token, Email = email });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            if (result)
            {
                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
                return RedirectToAction(nameof(Login));
            }
            TempData["ErrorMessage"] = "Không thể đặt lại mật khẩu. Vui lòng thử lại.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return View(model);
    }

    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Lỗi từ {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await HttpContext.AuthenticateAsync();
        if (!info.Succeeded)
        {
            TempData["ErrorMessage"] = "Không thể xác thực với provider.";
            return RedirectToAction(nameof(Login));
        }

        var claims = info.Principal.Claims.ToList();
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var provider = info.Properties?.Items["scheme"] ?? "Unknown";
        var providerKey = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
        {
            TempData["ErrorMessage"] = "Không thể lấy thông tin từ provider.";
            return RedirectToAction(nameof(Login));
        }

        try
        {
            var user = await _userService.GetUserByExternalLoginAsync(provider, providerKey);

            if (user == null)
            {
                var existingUser = await _userService.GetUserByEmailAsync(email);

                if (existingUser != null)
                {
                    await _userService.LinkExternalLoginAsync(existingUser.UserId, provider, providerKey, email, name, string.Empty);
                    user = existingUser;
                }
                else
                {
                    user = await _userService.CreateUserFromExternalLoginAsync(provider, providerKey, email, name, string.Empty);
                }
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.RoleName ?? "User");

            return user.IsAdmin
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng nhập: " + ex.Message;
            return RedirectToAction(nameof(Login));
        }
    }

    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

        if (user == null) return RedirectToAction(nameof(Login));

        return View(new RegisterViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(RegisterViewModel model)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid) return View("Profile", model);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        if (user == null) return RedirectToAction(nameof(Login));

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        user.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(model.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }

        await _context.SaveChangesAsync();
        HttpContext.Session.SetString("UserName", user.FullName);
        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdminUser()
    {
        try
        {
            var result = await _userService.CreateAdminUserAsync();
            TempData[result ? "SuccessMessage" : "InfoMessage"] = result
                ? "Tài khoản admin đã được tạo thành công!"
                : "Tài khoản admin đã tồn tại.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoles()
    {
        try
        {
            var result = await _userService.CreateRolesAsync();
            TempData[result ? "SuccessMessage" : "InfoMessage"] = result
                ? "Roles đã được tạo thành công!"
                : "Roles đã tồn tại.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> FixPasswordHashes()
    {
        try
        {
            var result = await _userService.FixPasswordHashesAsync();
            TempData[result ? "SuccessMessage" : "InfoMessage"] = result
                ? "Password hashes đã được sửa thành công!"
                : "Không có password hash nào cần sửa.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> TestUserInfo(string email)
    {
        try
        {
            var user = await _loginService.GetUserByEmailAsync(email);

            if (user == null) return Json(new { error = "User not found" });

            return Json(new
            {
                UserId = user.UserId.ToString(),
                FullName = user.FullName ?? "NULL",
                Email = user.Email ?? "NULL",
                RoleId = user.RoleId,
                RoleName = user.RoleName ?? "NULL",
                IsAdmin = user.IsAdmin,
                IsUser = user.IsUser,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TestAllUsers()
    {
        try
        {
            var statistics = await _loginService.GetUserStatisticsAsync();

            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    UserId = u.UserId.ToString(),
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : "NULL",
                    PasswordType = u.PasswordHash != null ?
                        (u.PasswordHash.StartsWith("$2a$") ? "BCrypt" : "Plain Text") : "NULL",
                    PasswordPreview = u.PasswordHash != null ?
                        u.PasswordHash.Substring(0, Math.Min(10, u.PasswordHash.Length)) + "..." : "NULL",
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return Json(new
            {
                users = users,
                count = users.Count,
                summary = new
                {
                    totalUsers = statistics.TotalUsers,
                    adminUsers = statistics.AdminUsers,
                    regularUsers = statistics.RegularUsers,
                    bcryptPasswords = statistics.BCryptPasswords,
                    plainTextPasswords = statistics.PlainTextPasswords,
                    nullPasswords = statistics.NullPasswords
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}
