using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSQueryOutputCachePolicy<TQuery, TResult>
    : CQRSOutputCachePolicy<TQuery>,
        ICQRSQueryOutputCachePolicy<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000")]
    public static TResult? GetResult(HttpContext context) =>
        ICQRSQueryOutputCachePolicy<TQuery, TResult>.GetResult(context);
}
