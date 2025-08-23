using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ShopTechnology.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. Request Path: {RequestPath}, Method: {RequestMethod}", 
                context.Request.Path, context.Request.Method);
            
            // Re-throw the exception to let the error handling middleware handle it
            throw;
        }
    }
}
