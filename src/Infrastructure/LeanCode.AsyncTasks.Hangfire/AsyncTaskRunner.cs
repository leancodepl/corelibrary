using System.Threading.Tasks;
using LeanCode.DomainModels.EventsExecution.Simple;

namespace LeanCode.AsyncTasks.Hangfire
{
    internal sealed class AsyncTaskRunner<TTask>
        where TTask : class, IAsyncTask
    {
        private readonly SimpleEventsExecutor executor;
        private readonly TTask task;

        public AsyncTaskRunner(SimpleEventsExecutor executor, TTask task)
        {
            this.executor = executor;
            this.task = task;
        }

        public Task Run() =>
            executor.HandleEventsOf(() => task.RunAsync());
    }

    internal sealed class AsyncTaskRunner<TTask, TParams>
        where TTask : class, IAsyncTask<TParams>
    {
        private readonly SimpleEventsExecutor executor;
        private readonly TTask task;

        public AsyncTaskRunner(SimpleEventsExecutor executor, TTask task)
        {
            this.executor = executor;
            this.task = task;
        }

        public Task Run(TParams @params) =>
            executor.HandleEventsOf(() => task.RunAsync(@params));
    }
}
