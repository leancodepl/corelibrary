using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public class SimpleEventsExecutor
    {
        private readonly PipelineExecutor<SimplePipelineContext, Func<Task>, Unit> exec;

        public SimpleEventsExecutor(IPipelineFactory factory)
        {
            var cfg = Pipeline.Build<SimplePipelineContext, Func<Task>, Unit>()
                .Use<EventsExecutorElement<SimplePipelineContext, Func<Task>, Unit>>()
                .Use<EventsInterceptorElement<SimplePipelineContext, Func<Task>, Unit>>()
                .Finalize<SimpleFinalizer>();
            exec = PipelineExecutor.Create(factory, cfg);
        }

        public Task HandleEventsOf(Func<Task> action)
        {
            var ctx = new SimplePipelineContext();
            return exec.ExecuteAsync(ctx, action);
        }
    }
}
