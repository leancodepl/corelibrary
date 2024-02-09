using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

internal interface IObjectExecutorFactory
{
    ObjectExecutor CreateExecutorFor(CQRSObjectKind kind, Type objectType, Type handlerType);
}

internal class ObjectExecutorFactory : IObjectExecutorFactory
{
    private static readonly MethodInfo ExecuteCommandMethod = typeof(ObjectExecutorFactory).GetMethod(
        nameof(ExecuteCommandAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static readonly MethodInfo ExecuteQueryMethod = typeof(ObjectExecutorFactory).GetMethod(
        nameof(ExecuteQueryAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static readonly MethodInfo ExecuteOperationMethod = typeof(ObjectExecutorFactory).GetMethod(
        nameof(ExecuteOperationAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    public ObjectExecutor CreateExecutorFor(CQRSObjectKind kind, Type objectType, Type handlerType)
    {
        return kind switch
        {
            CQRSObjectKind.Command
                => ExecuteCommandMethod.MakeGenericMethod(objectType, handlerType).CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Query
                => ExecuteQueryMethod
                    .MakeGenericMethod(objectType, GetResultType(typeof(IQuery<>)), handlerType)
                    .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Operation
                => ExecuteOperationMethod
                    .MakeGenericMethod(objectType, GetResultType(typeof(IOperation<>)), handlerType)
                    .CreateDelegate<ObjectExecutor>(),
            _ => throw new InvalidOperationException($"Unexpected object kind: {kind}.")
        };

        Type GetResultType(Type interfaceType) =>
            objectType
                .GetInterfaces()
                .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .GenericTypeArguments.First();
    }

    private static async Task<object?> ExecuteOperationAsync<TOperation, TResult, THandler>(
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TOperation : IOperation<TResult>
        where THandler : IOperationHandler<TOperation, TResult>
    {
        var operation = (TOperation)payload.Payload;

        var handler =
            httpContext.RequestServices.GetService<THandler>()
            ?? throw new OperationHandlerNotFoundException(typeof(TOperation));
        return await handler.ExecuteAsync(httpContext, operation);
    }

    private static async Task<object?> ExecuteQueryAsync<TQuery, TResult, THandler>(
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TQuery : IQuery<TResult>
        where THandler : IQueryHandler<TQuery, TResult>
    {
        var query = (TQuery)payload.Payload;

        var handler =
            httpContext.RequestServices.GetService<THandler>()
            ?? throw new QueryHandlerNotFoundException(typeof(TQuery));
        return await handler.ExecuteAsync(httpContext, query);
    }

    private static async Task<object?> ExecuteCommandAsync<TCommand, THandler>(
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TCommand : ICommand
        where THandler : ICommandHandler<TCommand>
    {
        var command = (TCommand)payload.Payload;

        var handler =
            httpContext.RequestServices.GetService<THandler>()
            ?? throw new CommandHandlerNotFoundException(typeof(TCommand));
        await handler.ExecuteAsync(httpContext, command);

        return CommandResult.Success;
    }
}
