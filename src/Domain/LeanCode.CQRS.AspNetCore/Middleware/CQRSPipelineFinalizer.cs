using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal static class CQRSPipelineFinalizer
{
    public static async Task HandleAsync(HttpContext context)
    {
        var cqrsEndpoint = context.GetCQRSEndpoint();
        var payload = context.GetCQRSRequestPayload();

        var result = await cqrsEndpoint.ObjectExecutor(context.RequestServices, payload);

        payload.SetResult(result);
    }
}
