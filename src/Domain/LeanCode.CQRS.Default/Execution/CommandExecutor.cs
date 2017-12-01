using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    public class CommandExecutor<TAppContext> : ICommandExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, CommandExecutionPayload, CommandResult> executor;

        public CommandExecutor(
            IPipelineFactory factory,
            CommandBuilder<TAppContext> config)
        {
            var cfg = Pipeline.Build<TAppContext, CommandExecutionPayload, CommandResult>()
                .Configure(new ConfigPipeline<TAppContext, CommandExecutionPayload, CommandResult>(config))
                .Finalize<CommandFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public Task<CommandResult> RunAsync<TContext, TCommand>(
            TAppContext appcontext,
            TContext context,
            TCommand command)
            where TCommand : ICommand<TContext>
        {
            var payload = new CommandExecutionPayload(context, command);
            return executor.ExecuteAsync(appcontext, payload);
        }
    }
}
