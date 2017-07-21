using LeanCode.Pipelines;

namespace LeanCode.Domain.Default.Tests.Security
{
    public class ExecutionContext : IPipelineContext
    {
        public IPipelineScope Scope { get; set; }
    }
}
