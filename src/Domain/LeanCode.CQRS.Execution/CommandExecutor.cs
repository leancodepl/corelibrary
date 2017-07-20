using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    using Executor = PipelineExecutor<ICommand, CommandResult>;
    using Builder = PipelineBuilder<ICommand, CommandResult>;

    public class CommandExecutor : ICommandExecutor
    {
        private readonly Executor executor;

        public CommandExecutor(
            IPipelineFactory factory,
            Func<Builder, Builder> config)
        {
            var cfg = Pipeline.Build<ICommand, CommandResult>()
                .Configure(config)
                .Finalize<CommandFinalizer>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public Task<CommandResult> RunAsync<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            return executor.ExecuteAsync(command);
        }
    }
}
