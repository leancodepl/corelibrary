using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        protected override async Task<IActionResult> ExecuteObjectAsync(object obj)
        {
            MethodInfo method;
            try
            {
                method = methodCache.GetOrAdd(obj.GetType(), GenerateMethod);
            }
            catch
            {
                // `Single` in `GenerateMethod` will throw if the query does not implement IRemoteQuery<>
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            var type = obj.GetType();
            var result = (Task<object>)method.Invoke(this, new[] { obj });
            try
            {
                var objResult = await result.ConfigureAwait(false);
                return new JsonResult(objResult);
            }
            catch (QueryHandlerNotFoundException)
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }

        private Task<object> ExecuteQuery<TQuery, TResult>(object cmd)
            where TQuery : IRemoteQuery<TResult>
        {
            return queryExecutor.GetAsync((TQuery)cmd).ContinueWith(e => (object)e.Result);
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
