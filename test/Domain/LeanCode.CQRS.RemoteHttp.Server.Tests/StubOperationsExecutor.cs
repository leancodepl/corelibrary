using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    public class StubOperationExecutor : IOperationExecutor<AppContext>
    {
        public AppContext? LastAppContext { get; private set; }
        public IOperation? LastOperation { get; private set; }

        public object? NextResult { get; set; }

        public Task<TResult> ExecuteAsync<TResult>(
            AppContext appContext,
            IOperation<TResult> operation)
        {
            LastAppContext = appContext;
            LastOperation = operation;
            if (NextResult is null)
            {
                return Task.FromResult(default(TResult)!);
            }
            else
            {
                return Task.FromResult((TResult)NextResult!);
            }
        }
    }
}
