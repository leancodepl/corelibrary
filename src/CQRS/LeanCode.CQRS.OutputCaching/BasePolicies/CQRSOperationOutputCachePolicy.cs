using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSOperationOutputCachePolicy<TOperation, TResult>
    : CQRSOutputCachePolicy<TOperation>,
        ICQRSOperationOutputCachePolicy<TOperation, TResult>
    where TOperation : IOperation<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000")]
    public static TResult? GetResult(HttpContext context) => ICQRSOperationOutputCachePolicy<TOperation, TResult>.GetResult(context);
}
