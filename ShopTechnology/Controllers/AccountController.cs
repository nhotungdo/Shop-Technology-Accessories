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
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    foreach (var userRole in user.UserRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
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

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = HashPassword(model.Password),
                    DateOfBirth = model.DateOfBirth,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign default role (Customer)
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
                if (customerRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = customerRole.RoleId,
                        AssignedAt = DateTime.Now
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
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
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
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
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                DateOfBirth = user.DateOfBirth,
                Avatar = user.Avatar,
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
                user.Address = model.Address;
                user.City = model.City;
                user.Province = model.Province;
                user.PostalCode = model.PostalCode;
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
                        new Role { Name = "Customer", CreatedAt = DateTime.Now }
                    };
                    _context.Roles.AddRange(roles);
                    await _context.SaveChangesAsync();
                }

                // Check if admin user exists
                var adminEmail = "admin@shoptech.com";
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

                if (adminUser == null)
                {
                    // Create admin user
                    adminUser = new User
                    {
                        FullName = "Admin",
                        Email = adminEmail,
                        PhoneNumber = "0123456789",
                        Password = HashPassword("admin123"),
                        DateOfBirth = new DateTime(1990, 1, 1),
                        CreatedAt = DateTime.Now
                    };
                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    // Assign admin role
                    var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                    if (adminRole != null)
                    {
                        var userRole = new UserRole
                        {
                            UserId = adminUser.UserId,
                            RoleId = adminRole.RoleId,
                            AssignedAt = DateTime.Now
                        };
                        _context.UserRoles.Add(userRole);
                        await _context.SaveChangesAsync();
                    }
                }

                // Check if customer user exists
                var customerEmail = "customer@shoptech.com";
                var customerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);

                if (customerUser == null)
                {
                    // Create customer user
                    customerUser = new User
                    {
                        FullName = "Customer",
                        Email = customerEmail,
                        PhoneNumber = "0987654321",
                        Password = HashPassword("customer123"),
                        DateOfBirth = new DateTime(1995, 1, 1),
                        CreatedAt = DateTime.Now
                    };
                    _context.Users.Add(customerUser);
                    await _context.SaveChangesAsync();

                    // Assign customer role
                    var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
                    if (customerRole != null)
                    {
                        var userRole = new UserRole
                        {
                            UserId = customerUser.UserId,
                            RoleId = customerRole.RoleId,
                            AssignedAt = DateTime.Now
                        };
                        _context.UserRoles.Add(userRole);
                        await _context.SaveChangesAsync();
                    }
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
