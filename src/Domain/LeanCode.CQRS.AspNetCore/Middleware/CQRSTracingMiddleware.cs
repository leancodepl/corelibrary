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
        var cqrsMetadata = httpContext.GetCQRSEndpoint().ObjectMetadata;

        using var activity = LeanCodeActivitySource.ActivitySource.StartActivity("pipeline.action");
        activity?.AddTag("object", cqrsMetadata.ObjectType.FullName);

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
