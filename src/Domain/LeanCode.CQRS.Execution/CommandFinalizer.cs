using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class CommandFinalizer
        : IPipelineFinalizer<ExecutionContext, ICommand, CommandResult>
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CommandFinalizer>();

        private readonly ICommandHandlerResolver resolver;

        public CommandFinalizer(ICommandHandlerResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            ExecutionContext ctx,
            ICommand command)
        {
            var commandType = command.GetType();
            var handler = resolver.FindCommandHandler(commandType);
            if (handler == null)
            {
                logger.Fatal(
                    "Cannot find a handler for the command {@Command}",
                    command);
                throw new CommandHandlerNotFoundException(commandType);
            }

            try
            {
                await handler.ExecuteAsync(command).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex, "Cannot execute command {@Command} because of internal error",
                    command);
                throw;
            }
            logger.Information(
                "Command {@Command} executed successfully", command);
            return CommandResult.Success();
        }
    }
}
