using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Services;
using ShopTechnology.DTOs;
using System.Security.Claims;
using LoginViewModel = ShopTechnology.ViewModels.LoginViewModel;
using RegisterViewModel = ShopTechnology.ViewModels.RegisterViewModel;
using ForgotPasswordDTO = ShopTechnology.DTOs.ForgotPasswordDTO;
using ResetPasswordDTO = ShopTechnology.DTOs.ResetPasswordDTO;
using CreateUserDTO = ShopTechnology.DTOs.CreateUserDTO;

namespace ShopTechnology.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IUserService _userService;

        public AccountController(ShopTechnologyAccessoriesContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FixPasswordHashes()
        {
            try
            {
                var result = await _userService.FixPasswordHashesAsync();
                if (result)
                {
                    TempData["SuccessMessage"] = "Password hashes đã được sửa thành công!";
                }
                else
                {
                    TempData["InfoMessage"] = "Không có password hash nào cần sửa.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"Login attempt for email: {model.Email}");

                    // Kiểm tra user tồn tại
                    var user = await _userService.GetUserByEmailAsync(model.Email);
                    if (user == null)
                    {
                        Console.WriteLine($"User not found: {model.Email}");
                        ModelState.AddModelError("", "Email không tồn tại trong hệ thống");
                        return View(model);
                    }

                    Console.WriteLine($"User found: {user.FullName}, Role: {user.RoleName}, IsAdmin: {user.IsAdmin}");

                    // Validate password
                    var isValid = await _userService.ValidateUserAsync(model.Email, model.Password);
                    Console.WriteLine($"Password validation result: {isValid}");

                    if (isValid)
                    {
                        Console.WriteLine($"Login successful for user: {user.FullName}");

                        // Lưu thông tin user vào session
                        HttpContext.Session.SetString("UserId", user.UserId.ToString());
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetString("UserName", user.FullName);
                        HttpContext.Session.SetString("UserRole", user.RoleName ?? "User");

                        Console.WriteLine($"Session data set - UserId: {user.UserId}, Role: {user.RoleName}");

                        // Redirect dựa trên role
                        if (user.IsAdmin)
                        {
                            Console.WriteLine($"Redirecting admin to dashboard");
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                        else
                        {
                            Console.WriteLine($"Redirecting user to home");
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Password validation failed for user: {user.FullName}");
                        ModelState.AddModelError("", "Mật khẩu không đúng");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login error: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng nhập: " + ex.Message);
                }
            }

            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra email đã tồn tại chưa
                    var emailExists = await _userService.IsEmailExistsAsync(model.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng");
                        return View(model);
                    }

                    // Tạo user mới
                    var createUserDto = new CreateUserDTO
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Password = model.Password,
                        PhoneNumber = model.PhoneNumber,
                        RoleId = 2 // Role User
                    };

                    await _userService.CreateUserAsync(createUserDto);

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký: " + ex.Message);
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Password Reset Actions
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.ForgotPasswordAsync(model.Email);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Đã gửi email hướng dẫn đặt lại mật khẩu. Vui lòng kiểm tra hộp thư của bạn.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Email không tồn tại trong hệ thống.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                }
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

            var model = new ResetPasswordDTO
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể đặt lại mật khẩu. Vui lòng thử lại.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(model);
        }

        // External Login Actions
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
                // Check if user exists with this external login
                var user = await _userService.GetUserByExternalLoginAsync(provider, providerKey);

                if (user == null)
                {
                    // Check if user exists with this email
                    var existingUser = await _userService.GetUserByEmailAsync(email);

                    if (existingUser != null)
                    {
                        // Link external login to existing user
                        await _userService.LinkExternalLoginAsync(existingUser.UserId, provider, providerKey, email, name, string.Empty);
                        user = existingUser;
                    }
                    else
                    {
                        // Create new user from external login
                        user = await _userService.CreateUserFromExternalLoginAsync(provider, providerKey, email, name, string.Empty);
                    }
                }

                // Set session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.RoleName ?? "User");

                // Redirect based on role
                if (user.IsAdmin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
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
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var profileViewModel = new RegisterViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(profileViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(RegisterViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
                if (user == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString("UserName", user.FullName);

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Profile));
            }

            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                var result = await _userService.CreateAdminUserAsync();
                if (result)
                {
                    TempData["SuccessMessage"] = "Tài khoản admin đã được tạo thành công!";
                }
                else
                {
                    TempData["InfoMessage"] = "Tài khoản admin đã tồn tại.";
                }
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
                if (result)
                {
                    TempData["SuccessMessage"] = "Roles đã được tạo thành công!";
                }
                else
                {
                    TempData["InfoMessage"] = "Roles đã tồn tại.";
                }
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
                var user = await _userService.GetUserByEmailAsync(email);
                if (user != null)
                {
                    var userInfo = new
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        RoleName = user.RoleName,
                        IsAdmin = user.IsAdmin,
                        IsUser = user.IsUser,
                        CreatedAt = user.CreatedAt
                    };

                    return Json(userInfo);
                }
                else
                {
                    return Json(new { error = "User not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


    }
}
