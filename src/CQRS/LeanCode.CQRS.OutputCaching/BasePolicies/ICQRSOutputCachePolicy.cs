using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public interface ICQRSOutputCachePolicy<TObject> : IOutputCachePolicy
    where TObject : notnull
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1000")]
    public static sealed TObject GetPayload(HttpContext context)
    {
        return (TObject)context.GetCQRSRequestPayload().Payload;
    }
}
