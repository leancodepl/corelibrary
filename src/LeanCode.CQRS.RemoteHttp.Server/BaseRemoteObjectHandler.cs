using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    abstract class BaseRemoteObjectHandler
    {
        private readonly JsonSerializer Serializer = new JsonSerializer();

        public Assembly TypesAssembly { get; }

        protected Serilog.ILogger Logger { get; }

        public BaseRemoteObjectHandler(Serilog.ILogger logger, Assembly typesAssembly)
        {
            TypesAssembly = typesAssembly;

            Logger = logger;
        }

        public async Task<IActionResult> ExecuteAsync(HttpRequest request)
        {
            var type = ExtractType(request);
            if (type == null)
            {
                Logger.Verbose("Cannot retrieve type from path {Path}, type not found", request.Path);
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            object obj;
            try
            {
                obj = DeserializeObject(type, request.Body);
            }
            catch (Exception ex)
            {
                Logger.Verbose(ex, "Cannot deserialize object body from the request stream");
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            if (obj == null)
            {
                Logger.Verbose("Client sent an empty object, ignoring");
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            IActionResult result;
            try
            {
                result = await ExecuteObjectAsync(obj);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Cannot execute object {@Object} of type {Type}", obj, type);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            Logger.Debug("Remote object of type {Type} executed successfully", type);
            return result;
        }

        protected abstract Task<IActionResult> ExecuteObjectAsync(object obj);

        private Type ExtractType(HttpRequest request)
        {
            var idx = request.Path.Value.LastIndexOf('/');
            var name = idx != -1 ? request.Path.Value.Substring(idx + 1) : request.Path.Value;
            return TypesAssembly.GetType(name);
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
