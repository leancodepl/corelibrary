using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.AspNetCore;

internal interface IObjectExecutorFactory
{
    ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object);
}

internal class ObjectExecutorFactory<TContext> : IObjectExecutorFactory
{
    private static readonly MethodInfo ExecuteCommandMethod = typeof(ObjectExecutorFactory<TContext>).GetMethod(
        nameof(ExecuteCommandAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static readonly MethodInfo ExecuteQueryMethod = typeof(ObjectExecutorFactory<TContext>).GetMethod(
        nameof(ExecuteQueryAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static readonly MethodInfo ExecuteOperationMethod = typeof(ObjectExecutorFactory<TContext>).GetMethod(
        nameof(ExecuteOperationAsync),
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    public ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object)
    {
        return @object.ObjectKind switch
        {
            CQRSObjectKind.Command
                => ExecuteCommandMethod.MakeGenericMethod(@object.ObjectType).CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Query
                => ExecuteQueryMethod
                    .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IQuery<>)))
                    .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Operation
                => ExecuteOperationMethod
                    .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IOperation<>)))
                    .CreateDelegate<ObjectExecutor>(),
            _ => throw new InvalidOperationException($"Unexpected object kind: {@object.ObjectKind}")
        };

        Type GetResultType(Type interfaceType) =>
            @object.ObjectType
                .GetInterfaces()
                .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .GenericTypeArguments.First();
    }

    private static async Task<object?> ExecuteOperationAsync<TOperation, TResult>(
        IServiceProvider sp,
        CQRSRequestPayload payload
    )
        where TOperation : IOperation<TResult>
    {
        var context = (TContext)payload.Context;
        var operation = (TOperation)payload.Payload;

        var handler = sp.GetRequiredService<IOperationHandler<TContext, TOperation, TResult>>();
        return await handler.ExecuteAsync(context, operation);
    }

    private static async Task<object?> ExecuteQueryAsync<TQuery, TResult>(
        IServiceProvider sp,
        CQRSRequestPayload payload
    )
        where TQuery : IQuery<TResult>
    {
        var context = (TContext)payload.Context;
        var query = (TQuery)payload.Payload;

        var handler = sp.GetRequiredService<IQueryHandler<TContext, TQuery, TResult>>();
        return await handler.ExecuteAsync(context, query);
    }

    private static async Task<object?> ExecuteCommandAsync<TCommand>(IServiceProvider sp, CQRSRequestPayload payload)
        where TCommand : ICommand
    {
        var context = (TContext)payload.Context;
        var command = (TCommand)payload.Payload;

        var handler = sp.GetRequiredService<ICommandHandler<TContext, TCommand>>();
        await handler.ExecuteAsync(context, command);

        return CommandResult.Success;
    }
}
