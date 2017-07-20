using System;
using System.Threading.Tasks;

namespace LeanCode.DomainModels.EventsExecutor
{
    public interface IEventsExecutor
    {
        Task<TResult> HandleEventsOf<TResult>(Func<Task<ExecutionResult<TResult>>> action);
    }

    public struct ExecutionResult<TResult>
    {
        public TResult Result { get; }
        public bool SkipExecution { get; }

        internal ExecutionResult(TResult result, bool skip)
        {
            Result = result;
            SkipExecution = skip;
        }
    }

    public static class ExecutionResult
    {
        public static ExecutionResult<Unit> Process()
            => new ExecutionResult<Unit>(Unit.Instance, false);

        public static ExecutionResult<Unit> Skip()
            => new ExecutionResult<Unit>(Unit.Instance, true);

        public static ExecutionResult<TResult> Process<TResult>(TResult result)
            => new ExecutionResult<TResult>(result, false);

        public static ExecutionResult<TResult> Skip<TResult>(TResult result)
            => new ExecutionResult<TResult>(result, true);
    }
}
