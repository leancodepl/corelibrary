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
                        httpContext.User.FindFirstValue(userIdClaim) is string userId
                        && !string.IsNullOrWhiteSpace(userId)
                    )
                    {
                        activity.AddBaggage(IdentityTraceBaggageHelpers.CurrentUserIdKey, userId);

                        if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.EndUserIdKey) is null)
                        {
                            activity.AddBaggage(IdentityTraceBaggageHelpers.EndUserIdKey, userId);
                        }
                    }

                    var userRoles = httpContext.User.Claims.Where(c => c.Type == roleClaim).Select(c => c.Value);

                    if (userRoles.Any())
                    {
                        activity.SetUserRoleBaggage(IdentityTraceBaggageHelpers.CurrentUserRoleKey, userRoles);

                        if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.EndUserRoleKey) is null)
                        {
                            activity.SetUserRoleBaggage(IdentityTraceBaggageHelpers.EndUserRoleKey, userRoles);
                        }
                    }
                }

                return next(httpContext);
            }
        );
    }
}
