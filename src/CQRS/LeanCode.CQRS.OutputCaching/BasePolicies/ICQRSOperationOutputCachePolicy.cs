using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public interface ICQRSOperationOutputCachePolicy<TOperation, TResult> : ICQRSOutputCachePolicy<TOperation>
    where TOperation : IOperation<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000")]
    public static TResult? GetResult(HttpContext context)
    {
        return context.GetCQRSExecutionResult()?.Payload is TResult result ? result : default;
    }
}
