using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSQueryOutputCachePolicy<TQuery, TResult>
    : CQRSOutputCachePolicy<TQuery>,
        ICQRSQueryOutputCachePolicy<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000")]
    public static TResult? GetResultPayload(HttpContext context) =>
        ICQRSQueryOutputCachePolicy<TQuery, TResult>.GetResultPayload(context);
}
