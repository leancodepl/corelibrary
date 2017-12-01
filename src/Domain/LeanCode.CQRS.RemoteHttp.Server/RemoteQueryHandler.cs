using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LeanCode.Components;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteQueryHandler<TAppContext>
        : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecQueryMethod = typeof(RemoteQueryHandler<TAppContext>)
            .GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly IQueryExecutor<TAppContext> queryExecutor;

        public RemoteQueryHandler(
            IQueryExecutor<TAppContext> queryExecutor,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
            : base(
                Serilog.Log.ForContext<RemoteQueryHandler<TAppContext>>(),
                catalog,
                contextTranslator)
        {
            this.queryExecutor = queryExecutor;
        }

        protected override async Task<ActionResult> ExecuteObjectAsync(
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
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }

            var type = obj.GetType();
            try
            {
                var result = (Task<object>)method.Invoke(this, new[] { context, obj });
                var objResult = await result.ConfigureAwait(false);
                return new ActionResult.Json(objResult);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
            catch (QueryHandlerNotFoundException)
            {
                return new ActionResult.StatusCode(StatusCodes.Status404NotFound);
            }
        }

        private async Task<object> ExecuteQuery<TContext, TQuery, TResult>(
            TAppContext appContext, object query)
            where TQuery : IRemoteQuery<TContext, TResult>
            where TContext : new()
        {
            var ctx = new TContext();
            if (ctx is IInitializeFromAppContext<TAppContext> ii)
            {
                ii.Initialize(appContext);
            }

            // TResult gets cast to object, so its necessary to await the Task.
            // ContinueWith will not propagate exceptions correctly.
            return await queryExecutor
                .GetAsync(appContext, ctx, (TQuery)query)
                .ConfigureAwait(false);
        }

        private static MethodInfo GenerateMethod(Type queryType)
        {
            var types = queryType.GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IRemoteQuery<,>))
                .GenericTypeArguments;
            return ExecQueryMethod.MakeGenericMethod(types[0], queryType, types[1]);
        }
    }
}
