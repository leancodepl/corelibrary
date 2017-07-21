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
    sealed class RemoteCommandHandler : BaseRemoteObjectHandler
    {
        private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler).GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ICommandExecutor commandExecutor;

        public RemoteCommandHandler(ICommandExecutor commandExecutor, TypesCatalog catalog)
            : base(Serilog.Log.ForContext<RemoteCommandHandler>(), catalog)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override async Task<ActionResult> ExecuteObjectAsync(
            ClaimsPrincipal user, object obj)
        {
            var type = obj.GetType();
            if (!typeof(IRemoteCommand).IsAssignableFrom(type))
            {
                Logger.Warning("The type {Type} is not an IRemoteCommand", type);
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            var method = methodCache.GetOrAdd(type, t => ExecCommandMethod.MakeGenericMethod(t));
            var result = (Task<CommandResult>)method.Invoke(this, new[] { user, obj });

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

        private Task<CommandResult> ExecuteCommand<TCommand>(ClaimsPrincipal user, object cmd)
            where TCommand : IRemoteCommand
        {
            return commandExecutor.RunAsync(user, (TCommand)cmd);
        }
    }
}
