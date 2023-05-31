using System.Diagnostics.CodeAnalysis;
using LeanCode.CQRS.AspNetCore.Serialization;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal class CQRSMiddleware
{
    private static readonly byte[] NullString = "null"u8.ToArray();

    private readonly ILogger logger = Log.ForContext<CQRSMiddleware>();

    private readonly ISerializer serializer;
    private readonly RequestDelegate next;

    public CQRSMiddleware(ISerializer serializer, RequestDelegate next)
    {
        this.serializer = serializer;
        this.next = next;
    }

    [SuppressMessage("?", "CA1031", Justification = "The handler is an exception boundary.")]
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsEndpoint = httpContext.GetCQRSEndpoint();

        var objectType = cqrsEndpoint.ObjectMetadata.ObjectType;
        object? obj;

        try
        {
            obj = await serializer.DeserializeAsync(httpContext.Request.Body, objectType, httpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", objectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", objectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        httpContext.SetCQRSRequestPayload(obj);

        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, objectType);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        await SerializeResultAsync(httpContext, cqrsEndpoint);
    }

    private async Task SerializeResultAsync(HttpContext httpContext, CQRSEndpointMetadata cqrsEndpoint)
    {
        var payload = httpContext.GetCQRSRequestPayload();

        if (payload.Result is null)
        {
            logger.Warning("CQRS execution ended with no result");
            return;
        }

        var result = payload.Result.Value;

        httpContext.Response.StatusCode = result.StatusCode;

        if (result.HasPayload)
        {
            httpContext.Response.ContentType = "application/json";
            if (result.Payload is null)
            {
                await httpContext.Response.Body.WriteAsync(NullString);
            }
            else
            {
                try
                {
                    await serializer.SerializeAsync(
                        httpContext.Response.Body,
                        result.Payload,
                        cqrsEndpoint.ObjectMetadata.ResultType,
                        httpContext.RequestAborted
                    );
                }
                catch (Exception ex)
                    when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
                {
                    logger.Warning(ex, "Failed to serialize response, request aborted");
                }
            }
        }
    }
}
