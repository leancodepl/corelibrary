using System;
using System.Threading.Tasks;

namespace LeanCode.Pipelines
{
    public interface IPipelineElement<TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(
            TInput input,
            Func<TInput, Task<TOutput>> next);
    }

    public interface IPipelineFinalizer<in TInput, TOutput>
    {
        Task<TOutput> ExecuteAsync(TInput input);
    }
}
