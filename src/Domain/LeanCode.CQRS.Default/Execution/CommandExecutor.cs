using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Default.Execution
{
    internal class CommandExecutor<TAppContext> : ICommandExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly PipelineExecutor<TAppContext, ICommand, CommandResult> executor;

        public CommandExecutor(IPipelineFactory factory, CommandBuilder<TAppContext> config)
        {
            var cfg = Pipeline
                .Build<TAppContext, ICommand, CommandResult>()
                .Configure(new ConfigPipeline<TAppContext, ICommand, CommandResult>(config))
                .Finalize<CommandFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public Task<CommandResult> RunAsync(TAppContext appContext, ICommand command) =>
            executor.ExecuteAsync(appContext, command);
    }
}
