using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public struct ExecutionContext : IPipelineContext
    {
        public IPipelineScope Scope { get; set; }
    }
}
