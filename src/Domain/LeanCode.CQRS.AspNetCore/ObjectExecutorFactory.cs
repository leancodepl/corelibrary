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
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TOperation : IOperation<TResult>
    {
        var operation = (TOperation)payload.Payload;

        var handler = httpContext.RequestServices.GetRequiredService<IOperationHandler<TOperation, TResult>>();
        return await handler.ExecuteAsync(httpContext, operation);
    }

    private static async Task<object?> ExecuteQueryAsync<TQuery, TResult>(
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TQuery : IQuery<TResult>
    {
        var query = (TQuery)payload.Payload;

        var handler = httpContext.RequestServices.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.ExecuteAsync(httpContext, query);
    }

    private static async Task<object?> ExecuteCommandAsync<TCommand>(
        HttpContext httpContext,
        CQRSRequestPayload payload
    )
        where TCommand : ICommand
    {
        var command = (TCommand)payload.Payload;

        var handler = httpContext.RequestServices.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.ExecuteAsync(httpContext, command);

        return CommandResult.Success;
    }
}
