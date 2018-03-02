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
    sealed class RemoteQueryHandler<TAppContext>
        : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecQueryMethod = typeof(RemoteQueryHandler<TAppContext>)
            .GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly IServiceProvider serviceProvider;

        public RemoteQueryHandler(
            IServiceProvider serviceProvider,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
            : base(catalog, contextTranslator)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task<ExecutionResult> ExecuteObjectAsync(
            TAppContext context, object obj)
        {
            MethodInfo method;
            try
            {
                method = methodCache.GetOrAdd(obj.GetType(), GenerateMethod);
            }
            catch
            {
                Logger.Warning("The type {Type} is not an IRemoteQuery", obj.GetType());
                // `Single` in `GenerateMethod` will throw if the query does not implement IRemoteQuery<>
                return ExecutionResult.Failed(StatusCodes.Status404NotFound);
            }

            var type = obj.GetType();
            try
            {
                var result = (Task<object>)method.Invoke(this, new[] { context, obj });
                var objResult = await result.ConfigureAwait(false);
                return ExecutionResult.Success(objResult);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default;
            }
            catch (QueryHandlerNotFoundException)
            {
                return ExecutionResult.Failed(StatusCodes.Status404NotFound);
            }
        }

        private async Task<object> ExecuteQuery<TContext, TQuery, TResult>(
            TAppContext appContext, object query)
            where TQuery : IRemoteQuery<TContext, TResult>
        {
            // TResult gets cast to object, so its necessary to await the Task.
            // ContinueWith will not propagate exceptions correctly.
            var queryExecutor = serviceProvider.GetService<IQueryExecutor<TAppContext>>();
            return await queryExecutor
                .GetAsync(appContext, (TQuery)query)
                .ConfigureAwait(false);
        }

        private static MethodInfo GenerateMethod(Type queryType)
        {
            var types = queryType.GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IRemoteQuery<,>))
                .GenericTypeArguments;

            var contextType = types[0];
            var resultType = types[1];
            return ExecQueryMethod.MakeGenericMethod(contextType, queryType, resultType);
        }
    }
}
