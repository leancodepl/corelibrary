using LeanCode.CQRS.AspNetCore.Local.Context;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

public static class HttpContextExtensions
{
    private static readonly ReadOnlyMemory<byte> NullString = "null"u8.ToArray();

    public static async Task SerializeCQRSResultAsync(this HttpContext httpContext)
    {
        if (httpContext.Response is NullHttpResponse)
        {
            throw new InvalidOperationException("Cannot serialize CQRS result to NullHttpResponse.");
        }

        var serializer = httpContext.RequestServices.GetRequiredService<ISerializer>();

        var objectMetadata = httpContext.GetCQRSObjectMetadata();
        var result = httpContext.GetCQRSRequiredExecutionResult();

        httpContext.Response.StatusCode = result.StatusCode;
        if (!result.HasPayload)
        {
            return;
        }

        httpContext.Response.ContentType = serializer.ContentType;
        if (result.Payload is null)
        {
            await httpContext.Response.Body.WriteAsync(NullString, httpContext.RequestAborted);
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
