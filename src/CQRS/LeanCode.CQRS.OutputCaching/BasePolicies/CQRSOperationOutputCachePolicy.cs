using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSOperationOutputCachePolicy<TOperation, TResult>
    : CQRSOutputCachePolicy<TOperation>,
        ICQRSOperationOutputCachePolicy<TOperation, TResult>
    where TOperation : IOperation<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000")]
    public static TResult? GetResultPayload(HttpContext context) =>
        ICQRSOperationOutputCachePolicy<TOperation, TResult>.GetResultPayload(context);
}
