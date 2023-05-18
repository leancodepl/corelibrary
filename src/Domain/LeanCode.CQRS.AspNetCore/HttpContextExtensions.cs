using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore;

public static class HttpContextExtensions
{
    public static CQRSEndpointMetadata GetCQRSEndpoint(this HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<CQRSEndpointMetadata>()
            ?? throw new InvalidOperationException("Request does not contain CQRSEndpointMetadata");
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
