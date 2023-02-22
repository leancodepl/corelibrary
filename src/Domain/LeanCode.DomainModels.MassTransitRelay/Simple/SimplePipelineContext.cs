using System;
using System.Threading;
using LeanCode.Pipelines;

namespace LeanCode.DomainModels.MassTransitRelay.Simple;

public sealed class SimplePipelineContext : IPipelineContext
{
    public IPipelineScope Scope { get; set; } = null!;
    public CancellationToken CancellationToken { get; set; }
}
