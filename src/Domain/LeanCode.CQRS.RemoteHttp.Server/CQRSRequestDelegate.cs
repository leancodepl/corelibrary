using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace LeanCode.CQRS.RemoteHttp.Server;

public class CQRSRequestDelegate<TContext>
    where TContext : IPipelineContext
{
    private static readonly byte[] NullString = "null"u8.ToArray();
    private readonly ILogger logger = Log.ForContext<CQRSRequestDelegate<TContext>>();

    private readonly ISerializer serializer;
    private readonly Func<HttpContext, TContext> contextTranslator;

    internal CQRSRequestDelegate(ISerializer serializer, Func<HttpContext, TContext> contextTranslator)
    {
        this.serializer = serializer;
        this.contextTranslator = contextTranslator;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var result = await ExecuteObjectAsync(context);
        await ExecuteResultAsync(result, context);
    }

    private async Task<ExecutionResult> ExecuteObjectAsync(HttpContext context)
    {
        var cqrsEndpoint = context.GetEndpoint()?.Metadata.GetMetadata<CQRSEndpointMetadata>();

        if (cqrsEndpoint is null)
        {
            logger.Error(
                "Request path {Path} does not contain CQRSEndpointMetadata, cannot execute",
                context.Request.Path
            );
            return ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }

        var objectType = cqrsEndpoint.ObjectMetadata.ObjectType;
        object? obj;

        try
        {
            obj = await serializer.DeserializeAsync(context.Request.Body, objectType, context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", objectType);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", objectType);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        var appContext = contextTranslator(context);
        ExecutionResult result;

        try
        {
            var payload = new CQRSPayload(obj, appContext);
            result = await cqrsEndpoint.Executor(context.RequestServices, payload);
        }
        catch (UnauthenticatedException)
        {
            result = ExecutionResult.Fail(StatusCodes.Status401Unauthorized);

            logger.Debug(
                "Unauthenticated user requested {@Object} of type {Type}, which requires authorization",
                obj,
                objectType
            );
        }
        catch (InsufficientPermissionException ex)
        {
            result = ExecutionResult.Fail(StatusCodes.Status403Forbidden);

            logger.Warning(
                "Authorizer {Authorizer} failed to authorize the user to run {@Object} of type {Type}",
                ex.AuthorizerName,
                obj,
                objectType
            );
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
        {
            logger.Debug(ex, "Cannot execute object {@Object} of type {Type}, request was aborted", obj, objectType);
            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, objectType);

            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }

        if (result.StatusCode >= 100 && result.StatusCode < 300)
        {
            logger.Debug("Remote object of type {Type} executed successfully", objectType);
        }

        return result;
    }

    private async Task ExecuteResultAsync(ExecutionResult result, HttpContext context)
    {
        context.Response.StatusCode = result.StatusCode;
        if (result.Succeeded)
        {
            context.Response.ContentType = "application/json";
            if (result.Payload is null)
            {
                await context.Response.Body.WriteAsync(NullString);
            }
            else
            {
                try
                {
                    await serializer.SerializeAsync(
                        context.Response.Body,
                        result.Payload,
                        result.Payload.GetType(),
                        context.RequestAborted
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
