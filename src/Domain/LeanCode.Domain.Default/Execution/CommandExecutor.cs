using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.Pipelines;

namespace LeanCode.Domain.Default.Execution
{
    using Executor = PipelineExecutor<ExecutionContext, ICommand, CommandResult>;

    public class CommandExecutor : ICommandExecutor
    {
        private readonly Executor executor;

        public CommandExecutor(
            IPipelineFactory factory,
            CommandBuilder config)
        {
            var cfg = Pipeline.Build<ExecutionContext, ICommand, CommandResult>()
                .Configure(new ConfigPipeline<ExecutionContext, ICommand, CommandResult>(config))
                .Finalize<CommandFinalizer<ExecutionContext>>();

            executor = PipelineExecutor.Create(factory, cfg);
        }

        public Task<CommandResult> RunAsync<TCommand>(
            ClaimsPrincipal user, TCommand command)
            where TCommand : ICommand
        {
            var ctx = new ExecutionContext { User = user };
            return executor.ExecuteAsync(ctx, command);
        }
    }
}
