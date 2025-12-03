using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.Execution;

public static class HttpContextExtensions
{
    public static CQRSObjectMetadata GetCQRSObjectMetadata(this HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<CQRSObjectMetadata>()
            ?? httpContext.Features.Get<CQRSObjectMetadata>()
            ?? throw new InvalidOperationException("Request does not contain CQRSObjectMetadata.");
    }

    public static CQRSRequestPayload GetCQRSRequestPayload(this HttpContext httpContext)
    {
        return httpContext.Features.GetRequiredFeature<CQRSRequestPayload>();
    }

    public static TPayload GetCQRSRequestPayload<TPayload>(this HttpContext httpContext)
    {
        return (TPayload)httpContext.GetCQRSRequestPayload().Payload;
    }

    public static CQRSRequestPayload SetCQRSRequestPayload(this HttpContext httpContext, object payload)
    {
        var cqrsRequestPayload = new CQRSRequestPayload(payload);
        httpContext.Features.Set(cqrsRequestPayload);
        return cqrsRequestPayload;
    }

    public static ExecutionResult? GetCQRSExecutionResult(this HttpContext httpContext)
    {
        return httpContext.Features.Get<ExecutionResult?>();
    }

    public static TPayload? GetCQRSResultPayload<TPayload>(this HttpContext httpContext)
    {
        return httpContext.GetCQRSExecutionResult() is { Payload: { } payload } ? (TPayload)payload : default;
    }

    public static ExecutionResult GetCQRSRequiredExecutionResult(this HttpContext httpContext)
    {
        return httpContext.GetCQRSExecutionResult()
            ?? throw new InvalidOperationException("Execution result is not set in HttpContext features.");
    }

    public static TPayload GetCQRSRequiredResultPayload<TPayload>(this HttpContext httpContext)
    {
        return (TPayload)httpContext.GetCQRSRequiredExecutionResult().Payload!;
    }

    public static void SetCQRSExecutionResult(this HttpContext httpContext, ExecutionResult result)
    {
        if (httpContext.GetCQRSExecutionResult() is not null)
        {
            throw new InvalidOperationException("The CQRS execution result has been already set.");
        }

        httpContext.Features.Set<ExecutionResult?>(result);
    }

    public static void SetCQRSObjectMetadataForLocalExecution(this HttpContext httpContext, CQRSObjectMetadata metadata)
    {
        httpContext.Features.Set(metadata);
    }
}
