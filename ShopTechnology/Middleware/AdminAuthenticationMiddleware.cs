using Microsoft.AspNetCore.Http;

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
            var userId = context.Session.GetString("UserId");
            var userRole = context.Session.GetString("UserRole");

            // Nếu chưa đăng nhập hoặc không phải Admin
            if (string.IsNullOrEmpty(userId) || userRole != "Admin")
            {
                // Redirect về trang login chính
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await _next(context);
    }
}
