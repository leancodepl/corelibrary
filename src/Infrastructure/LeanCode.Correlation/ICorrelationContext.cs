using System;
using LeanCode.Pipelines;

namespace LeanCode.Correlation
{
    public interface ICorrelationContext : IPipelineContext
    {
        Guid CorrelationId { get; set; }
        Guid ExecutionId { get; set; }
    }
}
