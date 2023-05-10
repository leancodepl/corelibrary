using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LeanCode.CQRS.RemoteHttp.Server;

public class ObjectHandlerBuilder<TContext>
    where TContext : IPipelineContext
{
    private readonly ISerializer serializer;
    private readonly Func<HttpContext, TContext> contextTranslator;

    public ObjectHandlerBuilder(Func<HttpContext, TContext> contextTranslator, ISerializer serializer)
    {
        this.contextTranslator = contextTranslator;
        this.serializer = serializer;
    }

    public RequestDelegate Query<TQuery>()
        where TQuery : IQuery
    {
        throw new InvalidOperationException();
    }

    public RequestDelegate Operation<TOperation>()
        where TOperation : IQuery
    {
        throw new InvalidOperationException();
    }

    public RequestDelegate Command<TCommand>()
        where TCommand : ICommand
    {
        return new ObjectHandler<TContext, TCommand>(serializer, contextTranslator, Handle).Handle;

        async Task<ExecutionResult> Handle(HttpContext httpContext, TContext context, TCommand command)
        {
            try
            {
                var executor = httpContext.RequestServices.GetRequiredService<ICommandExecutor<TContext>>();

                var cmdResult = await executor.RunAsync(context, command);

                if (cmdResult.WasSuccessful)
                {
                    return ExecutionResult.Success(cmdResult);
                }
                else
                {
                    return ExecutionResult.Success(cmdResult, StatusCodes.Status422UnprocessableEntity);
                }
            }
            catch (CommandHandlerNotFoundException)
            {
                return ExecutionResult.Fail(StatusCodes.Status404NotFound);
            }
        }
    }
}

public class ObjectHandler<TContext, TObject>
    where TContext : IPipelineContext
{
    private static readonly byte[] NullString = "null"u8.ToArray();
    private readonly ILogger logger = Log.ForContext<ObjectHandler<TContext, TObject>>();

    private readonly ISerializer serializer;
    private readonly Func<HttpContext, TContext> contextTranslator;
    private readonly Func<HttpContext, TContext, TObject, Task<ExecutionResult>> executor;

    internal ObjectHandler(
        ISerializer serializer,
        Func<HttpContext, TContext> contextTranslator,
        Func<HttpContext, TContext, TObject, Task<ExecutionResult>> executor)
    {
        this.serializer = serializer;
        this.contextTranslator = contextTranslator;
        this.executor = executor;
    }

    public async Task Handle(HttpContext context)
    {
        var result = await ExecuteObjectAsync(context);
        await ExecuteResultAsync(result, context, null!);
    }

    private async Task<ExecutionResult> ExecuteObjectAsync(HttpContext context)
    {
        var type = typeof(TObject);
        TObject? obj;

        try
        {
            obj = (TObject?)await serializer.DeserializeAsync(context.Request.Body, type, context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", type);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        if (obj is null)
        {
            logger.Warning("Client sent an empty object for type {Type}, ignoring", type);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        var appContext = contextTranslator(context);
        ExecutionResult result;

        try
        {
            result = await executor(context, appContext, obj);
        }
        catch (UnauthenticatedException)
        {
            result = ExecutionResult.Fail(StatusCodes.Status401Unauthorized);

            logger.Debug(
                "Unauthenticated user requested {@Object} of type {Type}, which requires authorization",
                obj,
                type
            );
        }
        catch (InsufficientPermissionException ex)
        {
            result = ExecutionResult.Fail(StatusCodes.Status403Forbidden);

            logger.Warning(
                "Authorizer {Authorizer} failed to authorize the user to run {@Object} of type {Type}",
                ex.AuthorizerName,
                obj,
                type
            );
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
        {
            logger.Debug(ex, "Cannot execute object {@Object} of type {Type}, request was aborted", obj, type);
            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, type);

            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }

        if (result.StatusCode >= 100 && result.StatusCode < 300)
        {
            logger.Debug("Remote object of type {Type} executed successfully", type);
        }

        return result;
    }

    private async Task ExecuteResultAsync(ExecutionResult result, HttpContext context, RequestDelegate next)
    {
        if (result.Skipped)
        {
            await next(context);
        }
        else
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
}
