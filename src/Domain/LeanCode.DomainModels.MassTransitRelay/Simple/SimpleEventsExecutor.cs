using System;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Simple
{
    public sealed class SimpleEventsExecutor
    {
        private readonly PipelineExecutor<SimplePipelineContext, Func<Task>, ValueTuple> exec;

        public SimpleEventsExecutor(IPipelineFactory factory)
        {
            exec = PipelineExecutor.Create(factory, Pipeline
                .Build<SimplePipelineContext, Func<Task>, ValueTuple>()
                .Use<EventsPublisherElement<SimplePipelineContext, Func<Task>, ValueTuple>>()
                .Finalize<SimpleFinalizer>());
        }

        public Task HandleEventsOf(Func<Task> action, Guid? correlationId = null) =>
            exec.ExecuteAsync(
                new SimplePipelineContext
                {
                    ExecutionId = correlationId ?? Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                }, action);
    }
}
