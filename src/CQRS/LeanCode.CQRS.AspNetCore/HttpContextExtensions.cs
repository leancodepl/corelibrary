using LeanCode.CQRS.AspNetCore.Local.Context;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

public static class HttpContextExtensions
{
    private static readonly ReadOnlyMemory<byte> NullString = "null"u8.ToArray();

    public static async Task CompleteCQRSExecutionResult(
        this HttpContext httpContext,
        ExecutionResult result
    )
    {
        httpContext.Features.Set(result);

        if (httpContext.Response is NullHttpResponse)
        {
            return;
        }

        var serializer = httpContext.RequestServices.GetRequiredService<ISerializer>();
        await SerializeCQRSResultAsync(httpContext, result, serializer);
    }

    private static async Task SerializeCQRSResultAsync(
        HttpContext httpContext,
        ExecutionResult result,
        ISerializer serializer
    )
    {
        var objectMetadata = httpContext.GetCQRSObjectMetadata();

        httpContext.Response.StatusCode = result.StatusCode;
        if (!result.HasPayload)
        {
            return;
        }

        httpContext.Response.ContentType = serializer.ContentType;
        if (result.Payload is null)
        {
            await httpContext.Response.Body.WriteAsync(NullString);
        }
        else
        {
            await serializer.SerializeAsync(
                httpContext.Response.Body,
                result.Payload,
                objectMetadata.ResultType,
                httpContext.RequestAborted
            );
        }
    }
}
