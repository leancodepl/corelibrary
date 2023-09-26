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
                        activity.AddBaggage(IdentityTraceBaggageHelpers.UserIdKey, userId);

                        if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.InitiatorIdKey) is null)
                        {
                            activity.AddBaggage(IdentityTraceBaggageHelpers.InitiatorIdKey, userId);
                        }
                    }

                    var userRoles = httpContext.User.Claims.Where(c => c.Type == roleClaim).Select(c => c.Value);

                    if (userRoles.Any())
                    {
                        activity.SetUserRoleBaggage(IdentityTraceBaggageHelpers.UserRoleKey, userRoles);

                        if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.InitiatorRoleKey) is null)
                        {
                            activity.SetUserRoleBaggage(IdentityTraceBaggageHelpers.InitiatorRoleKey, userRoles);
                        }
                    }
                }

                return next(httpContext);
            }
        );
    }
}
