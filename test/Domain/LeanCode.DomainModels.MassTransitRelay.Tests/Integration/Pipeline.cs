using System.Threading;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Integration
{
    public class Context : IPipelineContext
    {
        public IPipelineScope Scope { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
