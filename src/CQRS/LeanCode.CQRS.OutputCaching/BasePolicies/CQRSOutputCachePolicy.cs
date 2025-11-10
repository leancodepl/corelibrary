using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

// TODO: Implement some sensible base policy behavior.
[SuppressMessage("?", "CA1725")]
public abstract class CQRSOutputCachePolicy<TObject> : ICQRSOutputCachePolicy<TObject>
    where TObject : notnull
{
    public virtual ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        context.EnableOutputCaching = true;
        context.AllowLocking = true;
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
