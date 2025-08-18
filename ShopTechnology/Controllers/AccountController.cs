using Microsoft.AspNetCore.Mvc;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ShopTechnology.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public AccountController(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Lưu thông tin user vào session
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role?.RoleName ?? "User");

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
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
                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Tạo user mới
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    RoleId = 2, // Role User
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
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
                    user.PasswordHash = HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString("UserName", user.FullName);

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Profile));
            }

            return View("Profile", model);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
