using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal static class CQRSPipelineFinalizer
{
    public static async Task HandleAsync(HttpContext context)
    {
        var metadata = context.GetCQRSObjectMetadata();
        var payload = context.GetCQRSRequestPayload();

        using var activity = LeanCodeActivitySource.StartMiddleware("Execution");

        var result = await metadata.ObjectExecutor(context, payload);

        payload.SetResult(ExecutionResult.WithPayload(result));
    }
}
