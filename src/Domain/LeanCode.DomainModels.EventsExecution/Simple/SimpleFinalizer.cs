using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public class SimpleFinalizer
        : IPipelineFinalizer<SimplePipelineContext, Func<Task>, Unit>
    {
        public async Task<Unit> ExecuteAsync(
            SimplePipelineContext ctx, Func<Task> input)
        {
            await input();
            return Unit.Instance;
        }
    }
}
