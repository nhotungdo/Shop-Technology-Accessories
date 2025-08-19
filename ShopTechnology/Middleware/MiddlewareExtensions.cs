using Microsoft.AspNetCore.Builder;

namespace ShopTechnology.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAdminAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AdminAuthenticationMiddleware>();
    }
}
