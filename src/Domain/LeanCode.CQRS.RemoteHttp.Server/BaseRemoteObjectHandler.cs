using System;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server;

internal abstract class BaseRemoteObjectHandler<TAppContext>
{
    private readonly Func<HttpContext, TAppContext> contextTranslator;
    private readonly ISerializer serializer;

    public TypesCatalog Catalog { get; }

    protected Serilog.ILogger Logger { get; }

    public BaseRemoteObjectHandler(
        TypesCatalog catalog,
        Func<HttpContext, TAppContext> contextTranslator,
        ISerializer serializer)
    {
        Logger = Serilog.Log.ForContext(GetType());
        Catalog = catalog;
        this.contextTranslator = contextTranslator;
        this.serializer = serializer;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The handler is an exception boundary.")]
    public async Task<ExecutionResult> ExecuteAsync(HttpContext context)
    {
        var request = context.Request;
        var type = ExtractType(request);

        if (type is null)
        {
            Logger.Debug("Cannot retrieve type from path {Path}, type not found", request.Path);

            return ExecutionResult.Skip;
        }

        object? obj;

        try
        {
            obj = await serializer.DeserializeAsync(request.Body, type, context.RequestAborted);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", type);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        if (obj is null)
        {
            Logger.Warning("Client sent an empty object for type {Type}, ignoring", type);

            return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
        }

        var appContext = contextTranslator(context);
        ExecutionResult result;

        try
        {
            result = await ExecuteObjectAsync(appContext, obj);
        }
        catch (UnauthenticatedException)
        {
            result = ExecutionResult.Fail(StatusCodes.Status401Unauthorized);

            Logger.Debug(
                "Unauthenticated user requested {@Object} of type {Type}, which requires authorization",
                obj,
                type);
        }
        catch (InsufficientPermissionException ex)
        {
            result = ExecutionResult.Fail(StatusCodes.Status403Forbidden);

            Logger.Warning(
                "Authorizer {Authorizer} failed to authorize the user to run {@Object} of type {Type}",
                ex.AuthorizerName,
                obj,
                type);
        }
        catch (Exception ex)
            when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
        {
            Logger.Debug(ex, "Cannot execute object {@Object} of type {Type}, request was aborted", obj, type);
            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, type);

            result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
        }

        if (result.StatusCode >= 100 && result.StatusCode < 300)
        {
            Logger.Debug("Remote object of type {Type} executed successfully", type);
        }

        return result;
    }

    protected abstract Task<ExecutionResult> ExecuteObjectAsync(TAppContext appContext, object obj);

    private Type? ExtractType(HttpRequest request)
    {
        var idx = request.Path.Value!.LastIndexOf('/');

        var name = idx != -1
            ? request.Path.Value[(idx + 1)..]
            : request.Path.Value;

        return Catalog.GetType(name);
    }
}
