using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LeanCode.Components;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    sealed class RemoteQueryHandler : BaseRemoteObjectHandler
    {
        private static readonly MethodInfo ExecQueryMethod = typeof(RemoteQueryHandler).GetMethod("ExecuteQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly ConcurrentDictionary<Type, MethodInfo> methodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly IQueryExecutor queryExecutor;

        public RemoteQueryHandler(IQueryExecutor queryExecutor, TypesCatalog catalog)
            : base(Serilog.Log.ForContext<RemoteQueryHandler>(), catalog)
        {
            this.queryExecutor = queryExecutor;
        }

        protected override async Task<ActionResult> ExecuteObjectAsync(object obj)
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
                var result = (Task<object>)method.Invoke(this, new[] { obj });
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

        private async Task<object> ExecuteQuery<TQuery, TResult>(object cmd)
            where TQuery : IRemoteQuery<TResult>
        {
            // TResult gets cast to object, so its necessary to await the Task.
            // ContinueWith will not propagate exceptions correctly.
            return await queryExecutor.GetAsync((TQuery)cmd).ConfigureAwait(false);
        }

        private static MethodInfo GenerateMethod(Type queryType)
        {
            var implemented = queryType.GetInterfaces()
                .Single(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IRemoteQuery<>));
            var resultType = implemented.GetGenericArguments()[0];

            return ExecQueryMethod.MakeGenericMethod(queryType, resultType);
        }
    }
}
