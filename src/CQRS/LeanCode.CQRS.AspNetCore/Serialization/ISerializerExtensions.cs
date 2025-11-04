using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Serialization;

public static class ISerializerExtensions
{
    private static readonly ReadOnlyMemory<byte> NullString = "null"u8.ToArray();

    public static async Task SerializeCQRSResultAsync(this ISerializer serializer, HttpContext httpContext)
    {
        var objectMetadata = httpContext.GetCQRSObjectMetadata();
        var payload = httpContext.GetCQRSRequestPayload();

        if (payload.Result is not { } result)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

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
