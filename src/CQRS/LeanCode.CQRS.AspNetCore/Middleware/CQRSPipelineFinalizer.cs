using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal static class CQRSPipelineFinalizer
{
    public static async Task HandleAsync(HttpContext context)
    {
        using var activity = LeanCodeActivitySource.StartMiddleware("Execution");
        var metadata = context.GetCQRSObjectMetadata();
        var payload = context.GetCQRSRequestPayload();

        var result = await metadata.ObjectExecutor(context, payload);

        await context.CompleteCQRSExecutionResult(ExecutionResult.WithPayload(result));
    }
}
