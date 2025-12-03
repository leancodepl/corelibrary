using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LeanCode.CQRS.AspNetCore.Serialization;
using LeanCode.CQRS.Execution;
using LeanCode.Logging;
using LeanCode.OpenTelemetry;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Middleware;

public class CQRSMiddleware
{
    private readonly CQRSMetrics metrics;
    private readonly ISerializer serializer;
    private readonly RequestDelegate next;
    private readonly ILogger<CQRSMiddleware> logger;

    public CQRSMiddleware(
        CQRSMetrics metrics,
        ISerializer serializer,
        RequestDelegate next,
        ILogger<CQRSMiddleware> logger
    )
    {
        this.metrics = metrics;
        this.serializer = serializer;
        this.next = next;
        this.logger = logger;
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
            MarkError(CQRSMetrics.SerializationFailure, activity, ex);
            return;
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", objectType);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            MarkError(CQRSMetrics.SerializationFailure, activity);
            return;
        }

        var requestPayload = httpContext.SetCQRSRequestPayload(obj);

        try
        {
            await next(httpContext);

            if (!httpContext.IsHttpResponseSet())
            {
                await httpContext.SerializeCQRSResultAsync();
            }
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

        // assuming that in other cases the middleware itself will log & report appropriate metric
        if (httpContext.Response.StatusCode < 400)
        {
            metrics.CQRSSuccess();
            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.Information(
                "{ObjectKind} {@Object} executed successfully",
                cqrsEndpoint.ObjectKind,
                requestPayload.Payload
            );
        }
    }

    private void MarkError(string failureReason, Activity? activity, Exception? exception = null)
    {
        metrics.CQRSFailure(failureReason);
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.AddTag(CQRSMetrics.FailureReasonKey, failureReason);

        if (exception is not null)
        {
            activity?.AddException(exception);
        }
    }
}
