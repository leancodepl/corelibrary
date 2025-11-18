using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSOutputCachePolicy<TObject> : ICQRSOutputCachePolicy<TObject>
    where TObject : notnull
{
    public virtual ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        context.EnableOutputCaching = true;
        context.AllowLocking = true;
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }
}
