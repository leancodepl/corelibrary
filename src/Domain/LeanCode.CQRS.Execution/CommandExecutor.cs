using System;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public delegate PipelineBuilder<TAppContext, ICommand, CommandResult> CommandBuilder<TAppContext>(
        PipelineBuilder<TAppContext, ICommand, CommandResult> builder)
        where TAppContext : IPipelineContext;

    public class CommandExecutor<TAppContext> : ICommandExecutor<TAppContext>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CommandExecutor<TAppContext>>();
        private readonly PipelineExecutor<TAppContext, ICommand, CommandResult> executor;

        public CommandExecutor(IPipelineFactory factory, CommandBuilder<TAppContext> config)
        {
            var cfg = Pipeline
                .Build<TAppContext, ICommand, CommandResult>()
                .Configure(new ConfigPipeline<TAppContext, ICommand, CommandResult>(config))
                .Finalize<CommandFinalizer<TAppContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public async Task<CommandResult> RunAsync(TAppContext appContext, ICommand command)
        {
            var res = await executor.ExecuteAsync(appContext, command);
            if (res.WasSuccessful)
            {
                logger.Information("Command {@Command} executed successfully", command);
            }

            return res;
        }
    }
}
