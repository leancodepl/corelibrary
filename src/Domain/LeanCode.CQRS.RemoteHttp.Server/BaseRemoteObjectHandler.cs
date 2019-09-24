using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    internal abstract class BaseRemoteObjectHandler<TAppContext>
    {
        private readonly Func<HttpContext, TAppContext> contextTranslator;

        public TypesCatalog Catalog { get; }

        protected Serilog.ILogger Logger { get; }

        public BaseRemoteObjectHandler(TypesCatalog catalog, Func<HttpContext, TAppContext> contextTranslator)
        {
            Logger = Serilog.Log.ForContext(GetType());
            Catalog = catalog;
            this.contextTranslator = contextTranslator;
        }

        public async Task<ExecutionResult> ExecuteAsync(HttpContext context)
        {
            var request = context.Request;
            var type = ExtractType(request);

            if (type is null)
            {
                Logger.Warning("Cannot retrieve type from path {Path}, type not found", request.Path);

                return ExecutionResult.Skip;
            }

            object? obj;

            try
            {
                obj = await JsonSerializer.DeserializeAsync(request.Body, type);
            }
            catch (Exception ex)
            {
                Logger.Information(ex, "Cannot deserialize object body from the request stream for type {Type}", type);

                return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
            }

            if (obj is null)
            {
                Logger.Information("Client sent an empty object for type {Type}, ignoring", type);

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
            }
            catch (InsufficientPermissionException)
            {
                result = ExecutionResult.Fail(StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Cannot execute object {@Object} of type {Type}", obj, type);

                result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
            }

            if (result.StatusCode >= 100 && result.StatusCode < 300)
            {
                Logger.Information("Remote object of type {Type} executed successfully", type);
            }

            return result;
        }

        protected abstract Task<ExecutionResult> ExecuteObjectAsync(TAppContext appContext, object obj);

        private Type? ExtractType(HttpRequest request)
        {
            var idx = request.Path.Value.LastIndexOf('/');

            var name = idx != -1
                ? request.Path.Value.Substring(idx + 1)
                : request.Path.Value;

            return Catalog.GetType(name);
        }
    }
}
