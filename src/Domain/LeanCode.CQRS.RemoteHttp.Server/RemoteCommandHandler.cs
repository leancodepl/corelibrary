using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    interface IRemoteCommandHandler
    {
        TypesCatalog Catalog { get; }
        Task<ActionResult> ExecuteAsync(HttpContext context);
    }

    sealed class RemoteCommandHandler<TAppContext>
        : BaseRemoteObjectHandler<TAppContext>, IRemoteCommandHandler
    {
        private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler<TAppContext>)
            .GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ICommandExecutor<TAppContext> commandExecutor;

        public RemoteCommandHandler(
            ICommandExecutor<TAppContext> commandExecutor,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
            : base(
                Serilog.Log.ForContext<RemoteCommandHandler<TAppContext>>(),
                catalog,
                contextTranslator)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override async Task<ActionResult> ExecuteObjectAsync(
            TAppContext context, object obj)
        {
            var type = obj.GetType();
            if (!typeof(IRemoteCommand).IsAssignableFrom(type))
            {
                Logger.Warning("The type {Type} is not an IRemoteCommand", type);
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            var method = methodCache.GetOrAdd(type, t => ExecCommandMethod.MakeGenericMethod(t));
            var result = (Task<CommandResult>)method.Invoke(this, new[] { context, obj });

            CommandResult cmdResult;
            try
            {
                cmdResult = await result.ConfigureAwait(false);
            }
            catch (CommandHandlerNotFoundException)
            {
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            if (cmdResult.WasSuccessful)
            {
                return new ActionResult.Json(cmdResult);
            }
            else
            {
                return new ActionResult.Json(cmdResult, StatusCodes.Status422UnprocessableEntity);
            }
        }

        private Task<CommandResult> ExecuteCommand<TCommand>(
            TAppContext context,
            object cmd)
            where TCommand : IRemoteCommand
        {
            return commandExecutor.RunAsync(context, (TCommand)cmd);
        }
    }
}
