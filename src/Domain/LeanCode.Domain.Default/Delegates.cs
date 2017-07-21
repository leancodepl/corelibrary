using LeanCode.CQRS;
using LeanCode.Domain.Default.Execution;
using LeanCode.Pipelines;

namespace LeanCode.Domain.Default
{
    using CommandPipeline = PipelineBuilder<ExecutionContext, ICommand, CommandResult>;
    using QueryPipeline = PipelineBuilder<ExecutionContext, IQuery, object>;

    public delegate CommandPipeline CommandBuilder(CommandPipeline builder);
    public delegate QueryPipeline QueryBuilder(QueryPipeline builder);
}
