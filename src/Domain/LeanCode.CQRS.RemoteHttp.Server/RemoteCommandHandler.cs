using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteCommandHandler<TAppContext>
        : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler<TAppContext>)
            .GetMethod("ExecuteCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
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
            if (!typeof(IRemoteCommand<>).IsAssignableFrom(type))
            {
                Logger.Warning("The type {Type} is not an IRemoteCommand", type);
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            var method = methodCache.GetOrAdd(type, MakeExecutorMethod);
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

        private Task<CommandResult> ExecuteCommand<TContext, TCommand>(
            TAppContext appContext,
            object cmd)
            where TCommand : IRemoteCommand<TContext>
        {
            return commandExecutor.RunAsync<TContext, TCommand>(appContext, (TCommand)cmd);
        }

        private static MethodInfo MakeExecutorMethod(Type commandType)
        {
            var types = commandType
                .GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .GenericTypeArguments;
            var contextType = types[0];
            if (contextType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException($"The context {contextType.Name} does not have public default constructor that is required by the RemoteCQRS module.");
            }
            return ExecCommandMethod.MakeGenericMethod(contextType, commandType);
        }
    }
}
