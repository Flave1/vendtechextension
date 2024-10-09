using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace vendtechext.BLL.Filters
{
    public class CustomAuthorizeFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}
