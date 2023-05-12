using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public static class CQRSRequestFinalizer
{
    public static async Task HandleAsync(HttpContext context)
    {
        var cqrsEndpoint = context.GetCQRSEndpoint();
        var payload = context.GetCQRSRequestPayload();

        var result = await cqrsEndpoint.ObjectExecutor(context.RequestServices, payload);

        payload.SetResult(result);
    }
}
