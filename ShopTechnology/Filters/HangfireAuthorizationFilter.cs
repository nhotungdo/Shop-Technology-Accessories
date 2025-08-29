using Hangfire.Dashboard;

namespace ShopTechnology.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // Check if user is authenticated
            if (!httpContext.User.Identity?.IsAuthenticated == true)
                return false;

            // Check if user is admin
            return httpContext.User.IsInRole("Admin");
        }
    }
}
