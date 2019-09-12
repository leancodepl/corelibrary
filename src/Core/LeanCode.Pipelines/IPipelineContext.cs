using System;

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
        private IPipelineScope? scope;

        public IPipelineScope Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }
    }
}
