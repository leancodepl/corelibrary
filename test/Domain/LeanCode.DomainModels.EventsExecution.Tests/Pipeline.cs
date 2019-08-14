using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Tests
{
    public class Context : IEventsContext
    {
        public List<IDomainEvent> SavedEvents { get; set; }
        public List<(IDomainEvent Event, Type Handler)> ExecutedHandlers { get; set; }
        public List<(IDomainEvent Event, Type Handler)> FailedHandlers { get; set; }
        public IPipelineScope Scope { get; set; }
    }

    internal class ExecFinalizer : IPipelineFinalizer<Context, Func<int>, int>
    {
        public Task<int> ExecuteAsync(Context ctx, Func<int> input)
        {
            return Task.FromResult(input());
        }
    }

    internal static class PipelineExtensions
    {
        public static Task<int> HandleEventsOf(
            this PipelineExecutor<Context, Func<int>, int> exec,
            Func<int> action)
        {
            return exec.ExecuteAsync(new Context(), action);
        }
    }
}
