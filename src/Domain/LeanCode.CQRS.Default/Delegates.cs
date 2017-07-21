using LeanCode.CQRS.Default.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    using CommandPipeline = PipelineBuilder<ExecutionContext, ICommand, CommandResult>;
    using QueryPipeline = PipelineBuilder<ExecutionContext, IQuery, object>;

    public delegate CommandPipeline CommandBuilder(CommandPipeline builder);
    public delegate QueryPipeline QueryBuilder(QueryPipeline builder);
}
