using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.EventsExecution.Simple
{
    public sealed class SimpleFinalizer : IPipelineFinalizer<SimplePipelineContext, Func<Task>, object?>
    {
        public async Task<object?> ExecuteAsync(SimplePipelineContext ctx, Func<Task> input)
        {
            await input();

            return null;
        }
    }
}
