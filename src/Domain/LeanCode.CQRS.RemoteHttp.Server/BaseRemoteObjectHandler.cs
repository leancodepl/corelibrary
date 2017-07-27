using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    abstract class BaseRemoteObjectHandler<TAppContext>
    {
        private readonly JsonSerializer Serializer = new JsonSerializer();

        private readonly Func<HttpContext, TAppContext> contextTranslator;
        public TypesCatalog Catalog { get; }

        protected Serilog.ILogger Logger { get; }

        public BaseRemoteObjectHandler(
            Serilog.ILogger logger, TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            Logger = logger;
            Catalog = catalog;
            this.contextTranslator = contextTranslator;
        }

        public async Task<ActionResult> ExecuteAsync(HttpContext context)
        {
            var request = context.Request;
            var type = ExtractType(request);
            if (type == null)
            {
                Logger.Verbose("Cannot retrieve type from path {Path}, type not found", request.Path);
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            object obj;
            try
            {
                obj = DeserializeObject(type, request.Body);
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex, "Cannot deserialize object body from the request stream");
                return new ActionResult.StatusCode(StatusCodes.Status400BadRequest);
            }

            if (obj == null)
            {
                Logger.Verbose("Client sent an empty object, ignoring");
                return new ActionResult.StatusCode(StatusCodes.Status400BadRequest);
            }

            ActionResult result;
            var appContext = contextTranslator(context);
            try
            {
                result = await ExecuteObjectAsync(appContext, obj);
            }
            catch (UnauthenticatedException)
            {
                result = new ActionResult.StatusCode(StatusCodes.Status401Unauthorized);
            }
            catch (InsufficientPermissionException)
            {
                result = new ActionResult.StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Cannot execute object {@Object} of type {Type}", obj, type);
                result = new ActionResult.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var isSuccess = true;
            switch (result)
            {
                case ActionResult.StatusCode sc:
                    isSuccess = sc.Code < 300;
                    break;
                case ActionResult.Json j:
                    isSuccess = j.Code < 300;
                    break;
            }

            if (isSuccess)
            {
                Logger.Debug("Remote object of type {Type} executed successfully", type);
            }

            return result;
        }

        protected abstract Task<ActionResult> ExecuteObjectAsync(
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
