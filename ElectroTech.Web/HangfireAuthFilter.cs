using Hangfire.Dashboard;

namespace ElectroTech.Web;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true
            && (http.User.IsInRole("Admin")
             || http.User.IsInRole("SuperAdmin"));
    }
}