using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.Execution;

public static class HttpContextExtensions
{
    public static CQRSObjectMetadata GetCQRSObjectMetadata(this HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<CQRSObjectMetadata>()
            ?? throw new InvalidOperationException("Request does not contain CQRSObjectMetadata.");
    }

    public static CQRSRequestPayload GetCQRSRequestPayload(this HttpContext httpContext)
    {
        return httpContext.Features.GetRequiredFeature<CQRSRequestPayload>();
    }

    public static void SetCQRSRequestPayload(this HttpContext httpContext, object payload)
    {
        httpContext.Features.Set(new CQRSRequestPayload(payload));
    }
}
