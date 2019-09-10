using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public sealed class SimpleEventsExecutor
    {
        private readonly PipelineExecutor<SimplePipelineContext, Func<Task>, object?> exec;

        public SimpleEventsExecutor(IPipelineFactory factory)
        {
            exec = PipelineExecutor.Create(factory, Pipeline
                .Build<SimplePipelineContext, Func<Task>, object?>()
                .Use<EventsExecutorElement<SimplePipelineContext, Func<Task>, object?>>()
                .Use<EventsInterceptorElement<SimplePipelineContext, Func<Task>, object?>>()
                .Finalize<SimpleFinalizer>());
        }

        public Task HandleEventsOf(Func<Task> action) =>
            exec.ExecuteAsync(new SimplePipelineContext(), action);
    }
}
