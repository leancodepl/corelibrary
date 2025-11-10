using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public interface ICQRSOutputCachePolicy<TObject> : IOutputCachePolicy
    where TObject : notnull;

public static class ICQRSOutputCachePolicyExtensions
{
    public static TObject GetPayload<TObject>(this ICQRSOutputCachePolicy<TObject> _, HttpContext context)
        where TObject : notnull
    {
        return (TObject)context.GetCQRSRequestPayload().Payload;
    }
}
