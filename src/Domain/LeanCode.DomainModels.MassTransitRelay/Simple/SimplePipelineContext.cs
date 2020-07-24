using System;
using System.Threading;
using LeanCode.Correlation;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Simple
{
    public sealed class SimplePipelineContext : IPipelineContext, ICorrelationContext
    {
        public IPipelineScope Scope { get; set; } = null!;
        public Guid CorrelationId { get; set; }
        public Guid ExecutionId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
