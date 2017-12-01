using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    public delegate PipelineBuilder<TAppContext, CommandExecutionPayload, CommandResult>
        CommandBuilder<TAppContext>(
            PipelineBuilder<TAppContext, CommandExecutionPayload, CommandResult> builder)
        where TAppContext : IPipelineContext;
    public delegate PipelineBuilder<TAppContext, QueryExecutionPayload, object>
        QueryBuilder<TAppContext>(
            PipelineBuilder<TAppContext, QueryExecutionPayload, object> builder)
        where TAppContext : IPipelineContext;
}
