using Hangfire.Dashboard;

namespace MiniBank.Api.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In development, allow all
            // In production, add proper authentication
            var httpContext = context.GetHttpContext();
            return httpContext.Request.Host.Host == "localhost" || 
                   httpContext.User.Identity?.IsAuthenticated == true;
        }
    }
}