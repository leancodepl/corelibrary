using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSTracingMiddleware
{
    private readonly RequestDelegate next;

    public CQRSTracingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsMetadata = httpContext.GetCQRSObjectMetadata();

        using var activity = LeanCodeActivitySource.Start(
            $"{cqrsMetadata.ObjectKind}Handler {cqrsMetadata.HandlerType.FullName}"
        );
        activity?.AddTag("object.kind", cqrsMetadata.ObjectKind.ToString());
        activity?.AddTag("object.type", cqrsMetadata.ObjectType.FullName);
        activity?.AddTag("object.handler", cqrsMetadata.HandlerType.FullName);

        try
        {
            await next(httpContext);
            activity?.SetStatus(Status.Ok);
        }
        catch
        {
            activity?.SetStatus(Status.Error);
            throw;
        }
    }
}
