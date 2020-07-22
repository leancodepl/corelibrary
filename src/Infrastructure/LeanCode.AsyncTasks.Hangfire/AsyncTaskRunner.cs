using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Simple;

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

        public Task RunAsync() =>
            executor.HandleEventsOfAsync(() => task.RunAsync());
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

        public Task RunAsync(TParams @params) =>
            executor.HandleEventsOfAsync(() => task.RunAsync(@params));
    }
}
