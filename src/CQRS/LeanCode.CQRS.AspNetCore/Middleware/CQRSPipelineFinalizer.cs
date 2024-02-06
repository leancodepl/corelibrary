using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal static class CQRSPipelineFinalizer
{
    public static async Task HandleAsync(HttpContext context)
    {
        var metadata = context.GetCQRSObjectMetadata();
        var payload = context.GetCQRSRequestPayload();

        var result = await metadata.ObjectExecutor(context, payload);

        payload.SetResult(ExecutionResult.WithPayload(result));
    }
}
