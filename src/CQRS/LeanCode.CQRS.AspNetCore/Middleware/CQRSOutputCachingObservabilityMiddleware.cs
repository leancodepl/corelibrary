using LeanCode.CQRS.Execution;
using LeanCode.CQRS.OutputCaching.Registration;
using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal class CQRSOutputCachingObservabilityMiddleware
{
    private readonly CQRSMetrics metrics;
    private readonly RequestDelegate next;
    private readonly ILogger<CQRSOutputCachingObservabilityMiddleware> logger;

    public CQRSOutputCachingObservabilityMiddleware(
        CQRSMetrics metrics,
        RequestDelegate next,
        ILogger<CQRSOutputCachingObservabilityMiddleware> logger
    )
    {
        this.metrics = metrics;
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (GetOutputCachingPolicyName(httpContext) is not { } policyName)
        {
            await next(httpContext);
            return;
        }

        using var activity = LeanCodeActivitySource.StartMiddleware("OutputCache");
        activity?.SetTag("output_cache.policy", CQRSOutputCachePolicyName.GetObjectTypeString(policyName));

        OutputCacheContext? cacheContext = null;

        // The output cache feature is being removed on the of the middleware, so we have to capture it here
        // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L211
        httpContext.Response.OnStarting(() =>
        {
            cacheContext ??= GetOutputCacheContext(httpContext);
            return Task.CompletedTask;
        });

        await next(httpContext);
        if (activity is not null && cacheContext is { EnableOutputCaching: true })
        {
            var payload = httpContext.GetCQRSRequestPayload();
            activity.SetTag("output_cache.locking_allowed", cacheContext.AllowLocking);
            activity.SetTag("output_cache.storage_allowed", cacheContext.AllowCacheStorage);
            activity.SetTag("output_cache.lookup_allowed", cacheContext.AllowCacheLookup);

            if (IsServedFromCache(httpContext))
            {
                metrics.CQRSOutputCacheHit();
                activity.SetTag("output_cache.served_from_cache", true);
                logger.Debug("Object {@Object} served from output cache", payload.Payload);
            }
            else
            {
                metrics.CQRSOutputCacheMiss();
                activity.SetTag("output_cache.served_from_cache", false);

                // Not served from cache, so if cache storage was allowed, it was actually stored
                if (cacheContext.AllowCacheStorage)
                {
                    activity.SetTag("output_cache.stored_in_cache", true);
                    logger.Debug("Object {@Object} stored in output cache", payload.Payload);
                }
                else
                {
                    activity.SetTag("output_cache.stored_in_cache", false);
                }
            }
        }
    }

    // Basing on the middleware logic
    // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L215
    private static string? GetOutputCachingPolicyName(HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<OutputCacheAttribute>()?.PolicyName;
    }

    // Basing on the middleware logic
    // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L482
    private static OutputCacheContext? GetOutputCacheContext(HttpContext httpContext)
    {
        return httpContext.Features.Get<IOutputCacheFeature>()?.Context;
    }

    // Kinda hacky, also basing on the middleware logic
    // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L286
    // https://github.com/dotnet/aspnetcore/blob/10ef00003cabb87fb2c00355ad472147679a5f99/src/Middleware/OutputCaching/src/OutputCacheMiddleware.cs#L307
    private static bool IsServedFromCache(HttpContext httpContext)
    {
        return httpContext.Response.StatusCode == StatusCodes.Status304NotModified
            || (httpContext.Response.Headers.TryGetValue(HeaderNames.Age, out var ageValues) && ageValues.Count > 0);
    }
}
