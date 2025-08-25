using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShopTechnology.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopTechnologyAccessoriesContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ShopTechnologyAccessoriesContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword && u.IsActive);

                if (user != null)
                {
                    // Debug: Log thông tin user và role
                    Console.WriteLine($"User found: {user.Email}");
                    Console.WriteLine($"User Role: {user.Role?.Name ?? "NULL"}");
                    Console.WriteLine($"User RoleId: {user.RoleId}");
                    Console.WriteLine($"User IsActive: {user.IsActive}");

                    // Tạo claims cho người dùng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                        new Claim("RoleId", user.RoleId.ToString()),
                        new Claim("UserId", user.UserId.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Lưu thông tin vào session để tương thích với layout hiện tại
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role?.Name ?? "User");
                    HttpContext.Session.SetString("RoleId", user.RoleId.ToString());

                    // Chuyển hướng dựa trên vai trò
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Debug: Log logic chuyển hướng
                    Console.WriteLine($"Checking role redirect for: {user.Role?.Name}");

                    // Chuyển hướng dựa trên vai trò
                    if (user.Role?.Name?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Console.WriteLine("Redirecting to Admin Dashboard");
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else if (user.Role?.Name?.Equals("User", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Console.WriteLine("Redirecting to User Dashboard");
                        return RedirectToAction("Dashboard", "User");
                    }
                    else
                    {
                        // Fallback cho các role khác hoặc role null
                        Console.WriteLine($"Unknown role '{user.Role?.Name}', redirecting to Home");
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(model);
                }

                // Check if phone already exists
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được sử dụng.");
                    return View(model);
                }

                // Get default role (User)
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Vui lòng thử lại sau.");
                    return View(model);
                }

                var user = new User
                {
                    RoleId = userRole.RoleId,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = HashPassword(model.Password),
                    DateOfBirth = model.DateOfBirth,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var token = GeneratePasswordResetToken();
                    user.PasswordResetToken = token;
                    user.PasswordResetExpiry = DateTime.Now.AddHours(24);
                    await _context.SaveChangesAsync();

                    var resetLink = Url.Action("ResetPassword", "Account",
                        new { email = user.Email, token = token }, Request.Scheme);

                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

                    TempData["SuccessMessage"] = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                    return RedirectToAction(nameof(Login));
                }

                // Don't reveal that the user does not exist
                TempData["SuccessMessage"] = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.PasswordResetToken == token &&
                u.PasswordResetExpiry > DateTime.Now);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.PasswordResetToken == model.Token &&
                    u.PasswordResetExpiry > DateTime.Now);

                if (user != null)
                {
                    user.Password = HashPassword(model.Password);
                    user.PasswordResetToken = null;
                    user.PasswordResetExpiry = null;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address ?? string.Empty,
                City = user.City ?? string.Empty,
                Province = user.Province ?? string.Empty,
                PostalCode = user.PostalCode ?? string.Empty,
                DateOfBirth = user.DateOfBirth,
                Avatar = user.Avatar ?? string.Empty,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address ?? string.Empty;
                user.City = model.City ?? string.Empty;
                user.Province = model.Province ?? string.Empty;
                user.PostalCode = model.PostalCode ?? string.Empty;
                user.DateOfBirth = model.DateOfBirth;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction(nameof(Login));
                }

                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound();
                }

                var currentPasswordHash = HashPassword(model.CurrentPassword);
                if (user.Password != currentPasswordHash)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                user.Password = HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Đăng xuất khỏi authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LogoutPost()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Đăng xuất khỏi authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GeneratePasswordResetToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);
        }

        [HttpGet]
        public async Task<IActionResult> DebugLogin(string email, string password)
        {
            try
            {
                // First check if database exists and has any users
                var totalUsers = await _context.Users.CountAsync();
                var totalRoles = await _context.Roles.CountAsync();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User not found",
                        totalUsers = totalUsers,
                        totalRoles = totalRoles,
                        suggestion = "Try creating users first with /Account/CreateDefaultUsers"
                    });
                }

                var hashedPassword = HashPassword(password);
                var isPasswordCorrect = user.Password == hashedPassword;

                return Json(new
                {
                    success = true,
                    userFound = true,
                    userEmail = user.Email,
                    userPasswordHash = user.Password,
                    inputPasswordHash = hashedPassword,
                    isPasswordCorrect = isPasswordCorrect,
                    message = isPasswordCorrect ? "Password is correct" : "Password is incorrect",
                    totalUsers = totalUsers,
                    totalRoles = totalRoles
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalRoles = await _context.Roles.CountAsync();
                var totalProducts = await _context.Products.CountAsync();
                var totalCategories = await _context.Categories.CountAsync();

                return Json(new
                {
                    success = true,
                    totalUsers = totalUsers,
                    totalRoles = totalRoles,
                    totalProducts = totalProducts,
                    totalCategories = totalCategories,
                    message = "Database connection successful"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FixPasswords()
        {
            try
            {
                // Get all users with plain text passwords
                var users = await _context.Users.ToListAsync();
                var updatedCount = 0;

                foreach (var user in users)
                {
                    // Check if password is not already hashed (simple check)
                    if (!user.Password.Contains("=") && user.Password.Length < 50)
                    {
                        var originalPassword = user.Password;
                        user.Password = HashPassword(originalPassword);
                        user.UpdatedAt = DateTime.Now;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    updatedCount = updatedCount,
                    message = $"Đã cập nhật {updatedCount} tài khoản với mật khẩu hash"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestLogin(string email, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var hashedPassword = HashPassword(password);
                var isPasswordCorrect = user.Password == hashedPassword;

                return Json(new
                {
                    success = true,
                    userFound = true,
                    userEmail = user.Email,
                    userPasswordHash = user.Password,
                    inputPasswordHash = hashedPassword,
                    isPasswordCorrect = isPasswordCorrect,
                    message = isPasswordCorrect ? "Password is correct" : "Password is incorrect"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugRoles()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                var users = await _context.Users.Include(u => u.Role).ToListAsync();

                return Json(new
                {
                    success = true,
                    roles = roles.Select(r => new { roleId = r.RoleId, roleName = r.Name }),
                    users = users.Select(u => new
                    {
                        userId = u.UserId,
                        email = u.Email,
                        roleId = u.RoleId,
                        roleName = u.Role?.Name ?? "NULL",
                        isActive = u.IsActive
                    }),
                    message = "Roles and users information retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugUserInfo(string email)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                return Json(new
                {
                    success = true,
                    user = new
                    {
                        userId = user.UserId,
                        email = user.Email,
                        fullName = user.FullName,
                        roleId = user.RoleId,
                        roleName = user.Role?.Name ?? "NULL",
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt
                    },
                    message = "User information retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateDefaultUsers()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Check if roles exist, if not create them
                if (!await _context.Roles.AnyAsync())
                {
                    var roles = new List<Role>
                    {
                        new Role { Name = "Admin", CreatedAt = DateTime.Now },
                        new Role { Name = "User", CreatedAt = DateTime.Now }
                    };
                    _context.Roles.AddRange(roles);
                    await _context.SaveChangesAsync();
                }

                // Check if admin user exists
                var adminEmail = "admin@shoptech.com";
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

                if (adminUser == null)
                {
                    // Get admin role
                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                    if (adminRole == null)
                    {
                        return Json(new { success = false, error = "Admin role not found" });
                    }

                    // Create admin user
                    adminUser = new User
                    {
                        RoleId = adminRole.RoleId,
                        FullName = "Admin",
                        Email = adminEmail,
                        PhoneNumber = "0123456789",
                        Password = HashPassword("admin123"),
                        DateOfBirth = new DateTime(1990, 1, 1),
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();
                }

                // Check if customer user exists
                var customerEmail = "customer@shoptech.com";
                var customerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);

                if (customerUser == null)
                {
                    // Get user role
                    var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                    if (userRole == null)
                    {
                        return Json(new { success = false, error = "User role not found" });
                    }

                    // Create customer user
                    customerUser = new User
                    {
                        RoleId = userRole.RoleId,
                        FullName = "Customer",
                        Email = customerEmail,
                        PhoneNumber = "0987654321",
                        Password = HashPassword("customer123"),
                        DateOfBirth = new DateTime(1995, 1, 1),
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };
                    _context.Users.Add(customerUser);
                    await _context.SaveChangesAsync();
                }

                var userCount = await _context.Users.CountAsync();
                var roleCount = await _context.Roles.CountAsync();

                return Json(new
                {
                    success = true,
                    message = "Default users created successfully",
                    userCount = userCount,
                    roleCount = roleCount,
                    adminEmail = adminEmail,
                    adminPassword = "admin123",
                    customerEmail = customerEmail,
                    customerPassword = "customer123"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    message = "Failed to create default users"
                });
            }
        }
    }
}
