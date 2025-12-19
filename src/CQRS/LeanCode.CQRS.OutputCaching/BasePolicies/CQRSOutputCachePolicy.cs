using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSOutputCachePolicy<TObject> : ICQRSOutputCachePolicy<TObject>
    where TObject : notnull
{
    /// <inheritdoc cref="CacheRequestAsync"/>
    /// <remarks>
    /// Locking enabled by default.
    /// </remarks>
    public abstract ValueTask CacheRequestCoreAsync(OutputCacheContext context, CancellationToken cancellation);

    /// <inheritdoc />
    public async ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        context.EnableOutputCaching = true;
        context.AllowLocking = true;

        await CacheRequestCoreAsync(context, cancellation);

        // If that's false no `ServeFromCacheAsync` nor `ServeResponseAsync` will be called
        if (!context.AllowCacheLookup && !context.AllowCacheStorage)
        {
            RecordSpanTagsAndMetrics(false, context);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Unsuccessful responses with status codes other than 2xx are not cached by default.
    /// </remarks>
    public virtual ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        if (
            context.HttpContext.Response.StatusCode
            is < StatusCodes.Status200OK
                or >= StatusCodes.Status300MultipleChoices
        )
        {
            context.AllowCacheStorage = false;
        }

        RecordSpanTagsAndMetrics(true, context);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public virtual ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        RecordSpanTagsAndMetrics(false, context);
        return ValueTask.CompletedTask;
    }

    protected static void RecordSpanTagsAndMetrics(bool servingFromCache, OutputCacheContext context)
    {
        if (!context.EnableOutputCaching)
        {
            return;
        }

        var activity = Activity.Current;

        if (
            activity is null
            || !activity.OperationName.Contains(
                CQRSOutputCachingConsts.MiddlewareName,
                StringComparison.InvariantCulture
            )
        )
        {
            return;
        }

        activity.SetTag("output_cache.locking_allowed", context.AllowLocking);
        activity.SetTag("output_cache.storage_allowed", context.AllowCacheStorage);
        activity.SetTag("output_cache.lookup_allowed", context.AllowCacheLookup);

        if (servingFromCache)
        {
            CQRSOutputCacheMetrics.CacheHit();
            activity.SetTag("output_cache.served_from_cache", true);
        }
        else
        {
            CQRSOutputCacheMetrics.CacheMiss();
            activity.SetTag("output_cache.served_from_cache", false);

            // Not served from cache, so if cache storage was allowed, it was actually stored
            activity.SetTag("output_cache.stored_in_cache", context.AllowCacheStorage);
        }
    }
}
