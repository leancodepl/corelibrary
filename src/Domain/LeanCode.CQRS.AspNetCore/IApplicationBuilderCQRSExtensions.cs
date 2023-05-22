using LeanCode.CQRS.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LeanCode.CQRS.AspNetCore;

public static class IApplicationBuilderCQRSExtensions
{
    public static IApplicationBuilder Validate(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CQRSValidationMiddleware>();
    }

    public static IApplicationBuilder Secure(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CQRSSecurityMiddleware>();
    }

    public static IApplicationBuilder LogCQRSResponsesOnNonProduction(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NonProductionResponseLoggerMiddleware>();
    }

    public static IApplicationBuilder LogCQRSResponses(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseLoggerMiddleware>();
    }
}
