using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

internal interface IObjectExecutorFactory
{
    ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object);
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

    public ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object)
    {
        return @object.ObjectKind switch
        {
            CQRSObjectKind.Command
                => ExecuteCommandMethod
                    .MakeGenericMethod(@object.ObjectType, @object.HandlerType)
                    .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Query
                => ExecuteQueryMethod
                    .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IQuery<>)), @object.HandlerType)
                    .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Operation
                => ExecuteOperationMethod
                    .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IOperation<>)), @object.HandlerType)
                    .CreateDelegate<ObjectExecutor>(),
            _ => throw new InvalidOperationException($"Unexpected object kind: {@object.ObjectKind}")
        };

        Type GetResultType(Type interfaceType) =>
            @object
                .ObjectType
                .GetInterfaces()
                .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .GenericTypeArguments
                .First();
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
