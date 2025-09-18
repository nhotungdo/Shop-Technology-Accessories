using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShopTechnology.Services;
using ShopTechnology.ViewModels;
using ShopTechnology.Models;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            var users = await _userService.GetAllUsersAsync(page, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var success = await _userService.DeactivateUserAsync(userId);
            if (success)
            {
                TempData["Success"] = "Người dùng đã được vô hiệu hóa thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi vô hiệu hóa người dùng.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(int userId, int roleId)
        {
            var success = await _userService.ChangeUserRoleAsync(userId, roleId);
            if (success)
            {
                TempData["Success"] = "Vai trò người dùng đã được cập nhật thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật vai trò người dùng.";
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _userService.GetAllRolesAsync();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên vai trò không được để trống.";
                return RedirectToAction("Roles");
            }

            var role = await _userService.CreateRoleAsync(name);
            if (role != null)
            {
                TempData["Success"] = "Vai trò đã được tạo thành công.";
            }
            else
            {
                TempData["Error"] = "Vai trò đã tồn tại hoặc có lỗi xảy ra.";
            }
            return RedirectToAction("Roles");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(int roleId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên vai trò không được để trống.";
                return RedirectToAction("Roles");
            }

            var success = await _userService.UpdateRoleAsync(roleId, name);
            if (success)
            {
                TempData["Success"] = "Vai trò đã được cập nhật thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật vai trò.";
            }
            return RedirectToAction("Roles");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var success = await _userService.DeleteRoleAsync(roleId);
            if (success)
            {
                TempData["Success"] = "Vai trò đã được xóa thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể xóa vai trò đang được sử dụng hoặc có lỗi xảy ra.";
            }
            return RedirectToAction("Roles");
        }

        // API endpoints for admin operations
        [HttpPost]
        public async Task<IActionResult> ApiDeactivateUser([FromBody] int userId)
        {
            var success = await _userService.DeactivateUserAsync(userId);
            return Json(new { Success = success, Message = success ? "Người dùng đã được vô hiệu hóa" : "Có lỗi xảy ra" });
        }

        [HttpPost]
        public async Task<IActionResult> ApiChangeUserRole([FromBody] ChangeRoleRequest request)
        {
            var success = await _userService.ChangeUserRoleAsync(request.UserId, request.RoleId);
            return Json(new { Success = success, Message = success ? "Vai trò đã được cập nhật" : "Có lỗi xảy ra" });
        }

        [HttpPost]
        public async Task<IActionResult> ApiCreateRole([FromBody] string name)
        {
            var role = await _userService.CreateRoleAsync(name);
            return Json(new { Success = role != null, Message = role != null ? "Vai trò đã được tạo" : "Vai trò đã tồn tại", Role = role });
        }

        [HttpPost]
        public async Task<IActionResult> ApiUpdateRole([FromBody] UpdateRoleRequest request)
        {
            var success = await _userService.UpdateRoleAsync(request.RoleId, request.Name);
            return Json(new { Success = success, Message = success ? "Vai trò đã được cập nhật" : "Có lỗi xảy ra" });
        }

        [HttpPost]
        public async Task<IActionResult> ApiDeleteRole([FromBody] int roleId)
        {
            var success = await _userService.DeleteRoleAsync(roleId);
            return Json(new { Success = success, Message = success ? "Vai trò đã được xóa" : "Không thể xóa vai trò đang được sử dụng" });
        }
    }

    public class ChangeRoleRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateRoleRequest
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
