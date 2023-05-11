using System.Reflection;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server;

internal delegate Task<ExecutionResult> ObjectExecutor(IServiceProvider serviceProvider, CQRSPayload payload);

internal interface IObjectExecutorFactory
{
    ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object);
}

internal class ObjectExecutorFactory<TContext> : IObjectExecutorFactory
    where TContext : IPipelineContext
{
    private static readonly MethodInfo ExecuteCommandMethod =
        typeof(ObjectExecutorFactory<TContext>).GetMethod(nameof(ExecuteCommandAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ExecuteQueryMethod =
        typeof(ObjectExecutorFactory<TContext>).GetMethod(nameof(ExecuteQueryAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo ExecuteOperationMethod =
        typeof(ObjectExecutorFactory<TContext>).GetMethod(nameof(ExecuteOperationAsync), BindingFlags.Static | BindingFlags.NonPublic)!;

    public ObjectExecutor CreateExecutorFor(CQRSObjectMetadata @object)
    {
        return @object.ObjectKind switch
        {
            CQRSObjectKind.Command => ExecuteCommandMethod
                .MakeGenericMethod(@object.ObjectType)
                .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Query => ExecuteQueryMethod
                .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IQuery<>)))
                .CreateDelegate<ObjectExecutor>(),
            CQRSObjectKind.Operation => ExecuteOperationMethod
                .MakeGenericMethod(@object.ObjectType, GetResultType(typeof(IOperation<>)))
                .CreateDelegate<ObjectExecutor>(),
            _ => throw new InvalidOperationException($"Unexpected object kind: {@object.ObjectKind}")
        };

        Type GetResultType(Type interfaceType) => @object.ObjectType
            .GetInterfaces()
            .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType)
            .GenericTypeArguments
            .First();
    }

    private static async Task<ExecutionResult> ExecuteOperationAsync<TOperation, TResult>(IServiceProvider sp, CQRSPayload payload)
        where TOperation : IOperation<TResult>
    {
        var context = (TContext)payload.Context;
        var operation = (TOperation)payload.Object;

        try
        {
            var executor = sp.GetRequiredService<IOperationExecutor<TContext>>();
            var result =  await executor.ExecuteAsync(context, operation);

            return ExecutionResult.Success(result);
        }
        catch (QueryHandlerNotFoundException)
        {
            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }
    }

    private static async Task<ExecutionResult> ExecuteQueryAsync<TQuery, TResult>(IServiceProvider sp, CQRSPayload payload)
        where TQuery : IQuery<TResult>
    {
        var context = (TContext)payload.Context;
        var query = (TQuery)payload.Object;
        try
        {
            var executor = sp.GetRequiredService<IQueryExecutor<TContext>>();
            var result =  await executor.GetAsync(context, query);

            return ExecutionResult.Success(result);
        }
        catch (QueryHandlerNotFoundException)
        {
            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }
    }

    private static async Task<ExecutionResult> ExecuteCommandAsync<TCommand>(IServiceProvider sp, CQRSPayload payload)
        where TCommand : ICommand
    {
        var context = (TContext)payload.Context;
        var command = (TCommand)payload.Object;

        try
        {
            var executor = sp.GetRequiredService<ICommandExecutor<TContext>>();

            var cmdResult = await executor.RunAsync(context, command);

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
}
