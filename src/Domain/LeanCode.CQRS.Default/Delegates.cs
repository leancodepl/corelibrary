using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default
{
    public delegate PipelineBuilder<TAppContext, ICommand, CommandResult>
        CommandBuilder<TAppContext>(
            PipelineBuilder<TAppContext, ICommand, CommandResult> builder)
        where TAppContext : IPipelineContext;
    public delegate PipelineBuilder<TAppContext, IQuery, object>
        QueryBuilder<TAppContext>(
            PipelineBuilder<TAppContext, IQuery, object> builder)
        where TAppContext : IPipelineContext;
}
