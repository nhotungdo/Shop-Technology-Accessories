using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ShopTechnology.Middleware;

public class AdminAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AdminAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Kiểm tra nếu đang truy cập Admin area
        if (path != null && path.StartsWith("/admin"))
        {
            // Kiểm tra xem user đã đăng nhập chưa
            if (!context.User.Identity.IsAuthenticated)
            {
                // Redirect về trang login chính
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Kiểm tra xem user có phải là Admin không
            if (!context.User.IsInRole("Admin"))
            {
                // Redirect về trang chủ với thông báo lỗi
                context.Response.Redirect("/?error=access_denied");
                return;
            }
        }

        await _next(context);
    }
}
