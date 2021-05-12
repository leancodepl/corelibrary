using System;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Security.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    internal abstract class BaseRemoteObjectHandler<TAppContext>
    {
        private readonly Func<HttpContext, TAppContext> contextTranslator;
        private readonly ISerializer serializer;

        public Type Type { get; }

        protected Serilog.ILogger Logger { get; }

        public BaseRemoteObjectHandler(
            Type type,
            Func<HttpContext, TAppContext> contextTranslator,
            ISerializer serializer)
        {
            Logger = Serilog.Log.ForContext(GetType());
            Type = type;
            this.contextTranslator = contextTranslator;
            this.serializer = serializer;
        }

        public async Task<ExecutionResult> ExecuteAsync(HttpContext context)
        {
            var request = context.Request;
            object? obj;

            try
            {
                obj = await serializer.DeserializeAsync(request.Body, Type, context.RequestAborted);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Cannot deserialize object body from the request stream for type {Type}", Type);

                return ExecutionResult.Fail(StatusCodes.Status400BadRequest);
            }

            if (obj is null)
            {
                Logger.Warning("Client sent an empty object for type {Type}, ignoring", Type);

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
                    Type);
            }
            catch (InsufficientPermissionException ex)
            {
                result = ExecutionResult.Fail(StatusCodes.Status403Forbidden);

                Logger.Warning(
                    "Authorizer {Authorizer} failed to authorize the user to run {@Object} of type {Type}",
                    ex.AuthorizerName,
                    obj,
                    Type);
            }
            catch (Exception ex)
                when (ex is OperationCanceledException || ex.InnerException is OperationCanceledException)
            {
                Logger.Debug(ex, "Cannot execute object {@Object} of type {Type}, request was aborted", obj, Type);
                result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot execute object {@Object} of type {Type}", obj, Type);

                result = ExecutionResult.Fail(StatusCodes.Status500InternalServerError);
            }

            if (result.StatusCode >= 100 && result.StatusCode < 300)
            {
                Logger.Debug("Remote object of type {Type} executed successfully", Type);
            }

            return result;
        }

        protected abstract Task<ExecutionResult> ExecuteObjectAsync(TAppContext appContext, object obj);
    }
}
