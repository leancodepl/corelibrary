using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    internal sealed class RemoteQueryHandler<TAppContext> : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecQueryMethod = typeof(RemoteQueryHandler<TAppContext>)
            .GetMethod(nameof(ExecuteQuery), BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new NullReferenceException($"Failed to find {nameof(ExecuteQuery)} method.");

        private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly IServiceProvider serviceProvider;

        public RemoteQueryHandler(
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

            MethodInfo method;

            try
            {
                method = MethodCache.GetOrAdd(type, GenerateMethod);
            }
            catch
            {
                // `Single` in `GenerateMethod` will throw if the query does not implement IRemoteQuery<>
                Logger.Warning("The type {Type} is not an IRemoteQuery", type);

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
                ExceptionDispatchInfo
                    .Capture(ex.InnerException)
                    .Throw();

                throw; // the `Throw` method above `DoesNotReturn` anyway
            }
            catch (QueryHandlerNotFoundException)
            {
                return ExecutionResult.Fail(StatusCodes.Status404NotFound);
            }
        }

        private async Task<object> ExecuteQuery<TQuery, TResult>(
            TAppContext appContext, object query)
            where TQuery : IRemoteQuery<TResult>
        {
            // TResult gets cast to object, so its necessary to await the Task.
            // ContinueWith will not propagate exceptions correctly.
            return await serviceProvider
                .GetService<IQueryExecutor<TAppContext>>()
                .GetAsync(appContext, (TQuery)query);
        }

        private static MethodInfo GenerateMethod(Type queryType)
        {
            return ExecQueryMethod.MakeGenericMethod(queryType, queryType
                .GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IRemoteQuery<>))
                .GenericTypeArguments[0]);
        }
    }
}
