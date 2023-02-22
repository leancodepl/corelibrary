using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace LeanCode.Logging;

public static class UserIdLogsCorrelationMiddleware
{
    public static IApplicationBuilder UseUserIdLogsCorrelation(
        this IApplicationBuilder builder,
        string? userIdClaim = null)
    {
        return builder.Use(async (httpCtx, next) =>
        {
            using var userId = UserId(httpCtx, userIdClaim);

            await next();
        });
    }

    private static IDisposable? UserId(HttpContext httpCtx, string? userIdClaim)
    {
        if (httpCtx?.User.Identity?.IsAuthenticated ?? false)
        {
            var userId = httpCtx.User.FindFirstValue(userIdClaim ?? "sub");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return LogContext.PushProperty("UserId", userId);
            }
        }

        return null;
    }
}
