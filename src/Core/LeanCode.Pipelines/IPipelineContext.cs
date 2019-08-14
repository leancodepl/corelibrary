namespace LeanCode.Pipelines
{
    public interface IPipelineContext
    {
        /// <summary>
        /// Managed by <see cref="PipelineExecutor{TContext, TInput, TOutput}" />.
        /// </summary>
        IPipelineScope Scope { get; set; }
    }

    public class PipelineContext : IPipelineContext
    {
        public IPipelineScope Scope { get; set; }
    }
}
