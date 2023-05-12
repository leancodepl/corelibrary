using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

internal class CQRSPipelineStart
{
    private static readonly byte[] NullString = "null"u8.ToArray();

    private readonly ILogger logger = Log.ForContext<CQRSPipelineStart>();

    private readonly ISerializer serializer;
    private readonly Func<HttpContext, object> contextTranslator;
    private readonly RequestDelegate next;

    public CQRSPipelineStart(
        Func<HttpContext, object> contextTranslator,
        ISerializer serializer,
        RequestDelegate next)
    {
        this.serializer = serializer;
        this.contextTranslator = contextTranslator;
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsEndpoint = httpContext.GetCQRSEndpoint();

        object? obj;

        try
        {
            obj = await serializer.DeserializeAsync(httpContext.Request.Body, cqrsEndpoint.ObjectType, httpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", cqrsEndpoint.ObjectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", cqrsEndpoint.ObjectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var context = contextTranslator(httpContext);
        httpContext.SetCQRSRequestPayload(context, obj);

        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, cqrsEndpoint.ObjectType);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = "application/json";

        var result = httpContext.GetCQRSRequestPayload().Result;

        if (result is null)
        {
            await httpContext.Response.Body.WriteAsync(NullString);
        }
        else
        {
            try
            {
                await serializer.SerializeAsync(
                    httpContext.Response.Body,
                    result,
                    result.GetType(),
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
