using LeanCode.CQRS.OutputCaching;
using LeanCode.CQRS.OutputCaching.Registration;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal class CQRSOutputCachingObservabilityMiddleware
{
    private readonly RequestDelegate next;

    public CQRSOutputCachingObservabilityMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        using var activity = LeanCodeActivitySource.StartMiddleware(CQRSOutputCachingConsts.MiddlewareName);
        if (GetOutputCachingPolicyName(httpContext) is { } policyName)
        {
            activity?.SetTag("output_cache.policy_present", true);
            activity?.SetTag("output_cache.policy", CQRSOutputCachePolicyName.GetObjectTypeString(policyName));
        }
        else
        {
            activity?.SetTag("output_cache.policy_present", false);
        }

        await next(httpContext);
    }

    // Basing on the middleware logic
    // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L215
    private static string? GetOutputCachingPolicyName(HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<OutputCacheAttribute>()?.PolicyName;
    }
}
