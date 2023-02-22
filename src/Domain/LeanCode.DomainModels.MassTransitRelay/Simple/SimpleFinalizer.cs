using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Simple;

public sealed class SimpleFinalizer : IPipelineFinalizer<SimplePipelineContext, Func<Task>, ValueTuple>
{
    public async Task<ValueTuple> ExecuteAsync(SimplePipelineContext ctx, Func<Task> input)
    {
        await input();

#pragma warning disable SA1141
        return ValueTuple.Create();
#pragma warning restore SA1141
    }
}
