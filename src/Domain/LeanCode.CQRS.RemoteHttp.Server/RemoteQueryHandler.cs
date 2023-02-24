using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server;

internal sealed class RemoteQueryHandler<TAppContext> : BaseRemoteObjectHandler<TAppContext>
{
    private static readonly MethodInfo ExecQueryMethod =
        typeof(RemoteQueryHandler<TAppContext>).GetMethod(
            nameof(ExecuteQueryAsync),
            BindingFlags.NonPublic | BindingFlags.Instance
        ) ?? throw new InvalidOperationException($"Failed to find {nameof(ExecuteQueryAsync)} method.");

    private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new();
    private readonly IServiceProvider serviceProvider;

    public RemoteQueryHandler(
        IServiceProvider serviceProvider,
        TypesCatalog catalog,
        Func<HttpContext, TAppContext> contextTranslator,
        ISerializer serializer
    )
        : base(catalog, contextTranslator, serializer)
    {
        this.serviceProvider = serviceProvider;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA1031",
        Justification = "The handler is an exception boundary."
    )]
    protected override async Task<ExecutionResult> ExecuteObjectAsync(TAppContext context, object obj)
    {
        var type = obj.GetType();

        MethodInfo method;

        try
        {
            method = MethodCache.GetOrAdd(type, GenerateMethod);
        }
        catch
        {
            // `Single` in `GenerateMethod` will throw if the query does not implement IQuery<>
            Logger.Warning("The type {Type} is not an IQuery`1", type);

            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }

        try
        {
            var result = (Task<object?>)method.Invoke(this, new[] { context, obj })!; // TODO: assert not null
            var objResult = await result;

            return ExecutionResult.Success(objResult);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();

            throw; // the `Throw` method above `DoesNotReturn` anyway
        }
        catch (QueryHandlerNotFoundException)
        {
            return ExecutionResult.Fail(StatusCodes.Status404NotFound);
        }
    }

    private async Task<object?> ExecuteQueryAsync<TQuery, TResult>(TAppContext appContext, object query)
        where TQuery : IQuery<TResult>
    {
        // TResult gets cast to object, so its necessary to await the Task.
        // ContinueWith will not propagate exceptions correctly.
        return await serviceProvider.GetService<IQueryExecutor<TAppContext>>()!.GetAsync(appContext, (TQuery)query);
    }

    private static MethodInfo GenerateMethod(Type queryType)
    {
        return ExecQueryMethod.MakeGenericMethod(
            queryType,
            queryType
                .GetInterfaces()
                .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
                .GenericTypeArguments[0]
        );
    }
}
