using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteCommandHandler : BaseRemoteObjectHandler
    {
        private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler).GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ICommandExecutor commandExecutor;

        public RemoteCommandHandler(ICommandExecutor commandExecutor, Assembly typesAssembly)
            : base(Serilog.Log.ForContext<RemoteCommandHandler>(), typesAssembly)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override async Task<IActionResult> ExecuteObjectAsync(object obj)
        {
            var type = obj.GetType();
            if (!typeof(IRemoteCommand).IsAssignableFrom(type))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            var method = methodCache.GetOrAdd(type, t => ExecCommandMethod.MakeGenericMethod(t));
            var result = (Task<CommandResult>)method.Invoke(this, new[] { obj });

            CommandResult cmdResult;
            try
            {
                cmdResult = await result.ConfigureAwait(false);
            }
            catch (CommandHandlerNotFoundException)
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            if (cmdResult.WasSuccessful)
            {
                return new JsonResult(cmdResult);
            }
            else
            {
                return new JsonResult(cmdResult, StatusCodes.Status422UnprocessableEntity);
            }
        }

        private Task<CommandResult> ExecuteCommand<TCommand>(object cmd)
            where TCommand : IRemoteCommand
        {
            return commandExecutor.ExecuteAsync((TCommand)cmd);
        }
    }
}
