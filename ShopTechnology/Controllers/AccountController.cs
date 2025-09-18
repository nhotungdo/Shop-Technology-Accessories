using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;
using ShopTechnology.ViewModels;
using ShopTechnology.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace ShopTechnology.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AccountController(ApplicationDbContext context, IUserService userService, IJwtService jwtService)
        {
            _context = context;
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userService.LoginUserAsync(model.Email, model.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role.Name)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản chưa được xác thực email.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userService.RegisterUserAsync(
                    model.FullName,
                    model.Email,
                    model.PhoneNumber,
                    model.Password,
                    model.DateOfBirth
                );

                if (user != null)
                {
                    TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email đã được sử dụng hoặc có lỗi xảy ra.");
                    return View(model);
                }
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
                var success = await _userService.ForgotPasswordAsync(model.Email);
                if (success)
                {
                    TempData["Info"] = "Link đặt lại mật khẩu đã được gửi đến email của bạn.";
                }
                else
                {
                    TempData["Error"] = "Email không tồn tại trong hệ thống hoặc tài khoản không hoạt động.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("Index", "Home");
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
                var success = await _userService.ResetPasswordAsync(model.Email, model.Token, model.Password);
                if (success)
                {
                    TempData["Success"] = "Mật khẩu đã được đặt lại thành công.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            var success = await _userService.VerifyEmailAsync(email, token);
            if (success)
            {
                TempData["Success"] = "Email đã được xác thực thành công! Bạn có thể đăng nhập ngay bây giờ.";
            }
            else
            {
                TempData["Error"] = "Token xác thực không hợp lệ hoặc đã hết hạn.";
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
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

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var updateModel = new UserUpdateModel
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Province = model.Province,
                    PostalCode = model.PostalCode,
                    DateOfBirth = model.DateOfBirth,
                    Avatar = model.Avatar
                };

                var success = await _userService.UpdateUserProfileAsync(userId, updateModel);
                if (success)
                {
                    TempData["Success"] = "Thông tin cá nhân đã được cập nhật thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin.";
                }
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (success)
                {
                    TempData["Success"] = "Mật khẩu đã được thay đổi thành công.";
                }
                else
                {
                    TempData["Error"] = "Mật khẩu hiện tại không đúng.";
                }
            }

            return RedirectToAction("Profile");
        }

        // API endpoints for JWT authentication
        [HttpPost]
        public async Task<IActionResult> ApiLogin([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.LoginUserAsync(model.Email, model.Password);
                if (user != null)
                {
                    var token = _jwtService.GenerateToken(user);
                    return Json(new LoginResponseModel
                    {
                        Success = true,
                        Token = token,
                        Message = "Đăng nhập thành công",
                        User = user
                    });
                }
            }

            return Json(new LoginResponseModel
            {
                Success = false,
                Message = "Email hoặc mật khẩu không đúng"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ApiRegister([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.RegisterUserAsync(
                    model.FullName,
                    model.Email,
                    model.PhoneNumber,
                    model.Password,
                    model.DateOfBirth
                );

                if (user != null)
                {
                    return Json(new { Success = true, Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực." });
                }
            }

            return Json(new { Success = false, Message = "Email đã được sử dụng hoặc có lỗi xảy ra" });
        }

        [HttpPost]
        public async Task<IActionResult> ApiSocialLogin([FromBody] SocialLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.SocialLoginAsync(model.Provider, model.SocialId, model.Email, model.FullName);
                if (user != null)
                {
                    var token = _jwtService.GenerateToken(user);
                    return Json(new LoginResponseModel
                    {
                        Success = true,
                        Token = token,
                        Message = "Đăng nhập thành công",
                        User = user
                    });
                }
            }

            return Json(new LoginResponseModel
            {
                Success = false,
                Message = "Có lỗi xảy ra khi đăng nhập"
            });
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (result.Succeeded)
            {
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var socialId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(socialId))
                {
                    var user = await _userService.SocialLoginAsync("Google", socialId, email, name);
                    if (user != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.FullName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role.Name)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        return RedirectToLocal(returnUrl);
                    }
                }
            }

            TempData["Error"] = "Có lỗi xảy ra khi đăng nhập bằng Google.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult FacebookLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("FacebookCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Facebook");
        }

        [HttpGet]
        public async Task<IActionResult> FacebookCallback(string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync("Facebook");
            if (result.Succeeded)
            {
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var socialId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(socialId))
                {
                    var user = await _userService.SocialLoginAsync("Facebook", socialId, email, name);
                    if (user != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.FullName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role.Name)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        return RedirectToLocal(returnUrl);
                    }
                }
            }

            TempData["Error"] = "Có lỗi xảy ra khi đăng nhập bằng Facebook.";
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
