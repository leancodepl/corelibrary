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

namespace LeanCode.CQRS.RemoteHttp.Server
{
    internal sealed class RemoteOperationHandler<TAppContext> : BaseRemoteObjectHandler<TAppContext>
    {
        private static readonly MethodInfo ExecOperationMethod = typeof(RemoteOperationHandler<TAppContext>)
            .GetMethod(nameof(ExecuteOperationAsync), BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Failed to find {nameof(ExecuteOperationAsync)} method.");

        private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new();
        private readonly IServiceProvider serviceProvider;

        public RemoteOperationHandler(
            IServiceProvider serviceProvider,
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator,
            ISerializer serializer)
            : base(catalog, contextTranslator, serializer)
        {
            this.serviceProvider = serviceProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1031", Justification = "The handler is an exception boundary.")]
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
                // `Single` in `GenerateMethod` will throw if the operation does not implement IOperation<>
                Logger.Warning("The type {Type} is not an IOperation`1", type);

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
            catch (OperationHandlerNotFoundException)
            {
                return ExecutionResult.Fail(StatusCodes.Status404NotFound);
            }
        }

        private async Task<object?> ExecuteOperationAsync<TOperation, TResult>(
            TAppContext appContext, object operation)
            where TOperation : IOperation<TResult>
        {
            // TResult gets cast to object, so its necessary to await the Task.
            // ContinueWith will not propagate exceptions correctly.
            return await serviceProvider
                .GetService<IOperationExecutor<TAppContext>>()!
                .ExecuteAsync(appContext, (TOperation)operation);
        }

        private static MethodInfo GenerateMethod(Type operationType)
        {
            return ExecOperationMethod.MakeGenericMethod(operationType, operationType
                .GetInterfaces()
                .Single(i =>
                    i.IsConstructedGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IOperation<>))
                .GenericTypeArguments[0]);
        }
    }
}
