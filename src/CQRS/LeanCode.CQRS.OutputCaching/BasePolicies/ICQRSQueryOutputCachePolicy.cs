using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public interface ICQRSQueryOutputCachePolicy<TQuery, TResult> : ICQRSOutputCachePolicy<TQuery>
    where TQuery : IQuery<TResult>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000")]
    public static sealed TResult? GetResultPayload(HttpContext context)
    {
        return context.GetCQRSExecutionResult()?.Payload is TResult result ? result : default;
    }
}
