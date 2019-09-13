using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public sealed class SimpleEventsExecutor
    {
        private readonly PipelineExecutor<SimplePipelineContext, Func<Task>, ValueTuple> exec;

        public SimpleEventsExecutor(IPipelineFactory factory)
        {
            exec = PipelineExecutor.Create(factory, Pipeline
                .Build<SimplePipelineContext, Func<Task>, ValueTuple>()
                .Use<EventsExecutorElement<SimplePipelineContext, Func<Task>, ValueTuple>>()
                .Use<EventsInterceptorElement<SimplePipelineContext, Func<Task>, ValueTuple>>()
                .Finalize<SimpleFinalizer>());
        }

        public Task HandleEventsOf(Func<Task> action) =>
            exec.ExecuteAsync(new SimplePipelineContext(), action);
    }
}
