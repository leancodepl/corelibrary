using LeanCode.CQRS.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LeanCode.CQRS.AspNetCore;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder Validate(
        this IApplicationBuilder builder
    )
    {
        return builder.UseMiddleware<CQRSValidationMiddleware>();
    }

    public static IApplicationBuilder Secure(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CQRSSecurityMiddleware>();
    }
}
