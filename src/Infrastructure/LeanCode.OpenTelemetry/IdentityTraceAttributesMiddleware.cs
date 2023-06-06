using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;

namespace LeanCode.OpenTelemetry;

public static class IdentityTraceAttributesMiddleware
{
    public static IApplicationBuilder UseIdentityTraceAttributes(
        this IApplicationBuilder builder,
        string userIdClaim = "sub",
        string roleClaim = "role"
    )
    {
        return builder.Use(
            (httpContext, next) =>
            {
                if ((httpContext.User.Identity?.IsAuthenticated ?? false) && Activity.Current is Activity activity)
                {
                    if (
                        activity.GetBaggageItem(IdentityTraceBaggageHelpers.UserIdKey) is null
                        && httpContext.User.FindFirstValue(userIdClaim) is string userId
                        && !string.IsNullOrWhiteSpace(userId)
                    )
                    {
                        activity.AddBaggage(IdentityTraceBaggageHelpers.UserIdKey, userId);
                    }

                    var userRoles = httpContext.User.Claims.Where(c => c.Type == roleClaim).Select(c => c.Value);

                    if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.RoleKey) is null && userRoles.Any())
                    {
                        activity.SetUserRoleBaggage(userRoles);
                    }
                }

                return next(httpContext);
            }
        );
    }
}
