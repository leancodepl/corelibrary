using System;
using System.IO;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    abstract class BaseRemoteObjectHandler<TAppContext>
    {
        private readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly Func<HttpContext, TAppContext> contextTranslator;
        public TypesCatalog Catalog { get; }

        protected Serilog.ILogger Logger { get; }

        public BaseRemoteObjectHandler(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
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
                Logger.Verbose("Cannot retrieve type from path {Path}, type not found", request.Path);
                return ExecutionResult.Skip();
            }

            object obj;
            try
            {
                obj = DeserializeObject(type, request.Body);
            }
            catch (Exception ex)
            {
                Logger.Information(ex, "Cannot deserialize object body from the request stream");
                return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
            }

            if (obj == null)
            {
                Logger.Information("Client sent an empty object, ignoring");
                return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
            }

            ExecutionResult result;
            var appContext = contextTranslator(context);
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
                Logger.Debug("Remote object of type {Type} executed successfully", type);
            }

            return result;
        }

        protected abstract Task<ExecutionResult> ExecuteObjectAsync(
            TAppContext appContext, object obj);

        private Type ExtractType(HttpRequest request)
        {
            var idx = request.Path.Value.LastIndexOf('/');
            var name = idx != -1 ? request.Path.Value.Substring(idx + 1) : request.Path.Value;
            return Catalog.GetType(name);
        }

        private object DeserializeObject(Type destType, Stream body)
        {
            using (var reader = new JsonTextReader(new StreamReader(body)))
            {
                return Serializer.Deserialize(reader, destType);
            }
        }
    }
}
