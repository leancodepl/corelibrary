using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server;

internal sealed class RemoteCommandHandler<TAppContext> : BaseRemoteObjectHandler<TAppContext>
{
    private static readonly MethodInfo ExecCommandMethod = typeof(RemoteCommandHandler<TAppContext>)
        .GetMethod(nameof(ExecuteCommandAsync), BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException($"Failed to find {nameof(ExecuteCommandAsync)} method.");

    private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new();
    private readonly IServiceProvider serviceProvider;

    public RemoteCommandHandler(
        IServiceProvider serviceProvider,
        TypesCatalog catalog,
        Func<HttpContext, TAppContext> contextTranslator,
        ISerializer serializer)
        : base(catalog, contextTranslator, serializer)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override async Task<ExecutionResult> ExecuteObjectAsync(TAppContext context, object obj)
    {
        var type = obj.GetType();

        if (!typeof(ICommand).IsAssignableFrom(type))
        {
            Logger.Warning("The type {Type} is not an ICommand", type);

            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }

        var method = MethodCache.GetOrAdd(type, MakeExecutorMethod);

        try
        {
            var result = (Task<CommandResult>)method.Invoke(this, new[] { context, obj })!; // TODO: assert not null
            var cmdResult = await result;

            if (cmdResult.WasSuccessful)
            {
                return ExecutionResult.Success(cmdResult);
            }
            else
            {
                return ExecutionResult.Success(cmdResult, StatusCodes.Status422UnprocessableEntity);
            }
        }
        catch (CommandHandlerNotFoundException)
        {
            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }
    }

    private Task<CommandResult> ExecuteCommandAsync<TCommand>(TAppContext appContext, object cmd)
        where TCommand : ICommand
    {
        var commandExecutor = serviceProvider.GetService<ICommandExecutor<TAppContext>>()!;

        return commandExecutor.RunAsync(appContext, (TCommand)cmd);
    }

    private static MethodInfo MakeExecutorMethod(Type commandType)
    {
        return ExecCommandMethod.MakeGenericMethod(commandType);
    }
}
