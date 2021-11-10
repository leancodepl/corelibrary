using System;
using System.Threading;

namespace LeanCode.Pipelines
{
    public interface IPipelineContext
    {
        /// <summary>
        /// Managed by <see cref="PipelineExecutor{TContext, TInput, TOutput}" />.
        /// </summary>
        IPipelineScope Scope { get; set; }
        CancellationToken CancellationToken { get; }
    }

    public class PipelineContext : IPipelineContext
    {
        private IPipelineScope? scope;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1065", Justification = "Expected behavior.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2201", Justification = "Expected behavior.")]
        public IPipelineScope Scope
        {
            get => scope ?? throw new NullReferenceException();
            set => scope = value;
        }

        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
