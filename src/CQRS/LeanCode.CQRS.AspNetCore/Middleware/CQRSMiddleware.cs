using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSMiddleware
{
    private static readonly byte[] NullString = "null"u8.ToArray();

    private readonly ILogger logger = Log.ForContext<CQRSMiddleware>();

    private readonly CQRSMetrics metrics;
    private readonly ISerializer serializer;
    private readonly RequestDelegate next;

    public CQRSMiddleware(CQRSMetrics metrics, ISerializer serializer, RequestDelegate next)
    {
        this.metrics = metrics;
        this.serializer = serializer;
        this.next = next;
    }

    [SuppressMessage("?", "CA1031", Justification = "The handler is an exception boundary.")]
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var cqrsEndpoint = httpContext.GetCQRSObjectMetadata();

        using var activity = LeanCodeActivitySource.StartExecution(
            cqrsEndpoint.ObjectKind.ToString(),
            cqrsEndpoint.HandlerType.FullName
        );
        activity?.AddTag("object.kind", cqrsEndpoint.ObjectKind.ToString());
        activity?.AddTag("object.type", cqrsEndpoint.ObjectType.FullName);
        activity?.AddTag("object.handler", cqrsEndpoint.HandlerType.FullName);

        var objectType = cqrsEndpoint.ObjectType;
        object? obj;

        try
        {
            obj = await serializer.DeserializeAsync(httpContext.Request.Body, objectType, httpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", objectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            MarkError(CQRSMetrics.InternalError, activity, ex);
            return;
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", objectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            MarkError(CQRSMetrics.SerializationFailure, activity);
            return;
        }

        httpContext.SetCQRSRequestPayload(obj);

        try
        {
            await next(httpContext);
            await SerializeResultAsync(httpContext, cqrsEndpoint, activity);
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
        {
            logger.Debug(ex, "{ObjectKind} {@Object} cancelled", objectType, obj);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, objectType);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            MarkError(CQRSMetrics.InternalError, activity, ex);
        }
    }

    private async Task SerializeResultAsync(HttpContext httpContext, CQRSObjectMetadata objectMetadata, Activity? activity)
    {
        var payload = httpContext.GetCQRSRequestPayload();

        if (payload.Result is null)
        {
            logger.Warning("CQRS execution ended with no result");
            MarkError(CQRSMetrics.InternalError, activity);
            return;
        }

        var result = payload.Result.Value;

        httpContext.Response.StatusCode = result.StatusCode;

        if (result.HasPayload)
        {
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

            if (httpContext.Response.StatusCode < 400)
            {
                // assuming that in other cases the middleware itself will log & report appropriate metric
                metrics.CQRSSuccess();
                activity?.SetStatus(ActivityStatusCode.Ok);
                logger.Information(
                    "{ObjectKind} {@Object} executed successfully",
                    objectMetadata.ObjectKind,
                    payload.Payload
                );
            }
        }
        else
        {
            MarkError(CQRSMetrics.InternalError, activity);
        }
    }

    private void MarkError(string failureReason, Activity? activity, Exception? exception = null)
    {
        metrics.CQRSFailure(CQRSMetrics.InternalError);
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddTag(CQRSMetrics.FailureReasonKey, failureReason);

        if (exception is not null)
        {
            activity?.AddException(exception);
        }
    }
}
