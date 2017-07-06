using System;
using System.Threading.Tasks;

namespace LeanCode.DomainModels.EventsExecutor
{
    public static class IEventsExecutorExtensions
    {
        public static Task HandleEventsOf(
            this IEventsExecutor executor,
            Func<Task<ExecutionResult<Unit>>> action)
        {
            return executor.HandleEventsOf(action);
        }
    }
}
