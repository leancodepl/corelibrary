using System;
using System.Threading.Tasks;

namespace LeanCode.Pipelines
{
    public interface IPipelineElement<TContext, TInput, TOutput>
        where TContext : IPipelineContext
    {
        Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next);
    }

    public interface IPipelineFinalizer<TContext, in TInput, TOutput>
        where TContext : IPipelineContext
    {
        Task<TOutput> ExecuteAsync(TContext ctx, TInput input);
    }
}
