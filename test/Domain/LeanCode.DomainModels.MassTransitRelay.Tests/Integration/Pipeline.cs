using System;
using System.Collections.Generic;
using System.Threading;
using LeanCode.Correlation;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class Context : ICorrelationContext
    {
        public Guid CorrelationId { get; set; }
        public Guid ExecutionId { get; set; }
        public IPipelineScope Scope { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
