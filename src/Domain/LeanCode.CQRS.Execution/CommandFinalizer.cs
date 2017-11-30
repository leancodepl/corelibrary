using System;
using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class CommandFinalizer<TContext>
        : IPipelineFinalizer<TContext, CommandExecutionPayload, CommandResult>
        where TContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CommandFinalizer<TContext>>();

        private readonly ICommandHandlerResolver resolver;

        public CommandFinalizer(ICommandHandlerResolver resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(
            TContext _, CommandExecutionPayload payload)
        {
            var context = payload.Context;
            var command = payload.Command;

            var contextType = context.GetType();
            var commandType = command.GetType();
            var handler = resolver.FindCommandHandler(contextType, commandType);
            if (handler == null)
            {
                logger.Fatal(
                    "Cannot find a handler for the command {@Command}",
                    command);
                throw new CommandHandlerNotFoundException(contextType, commandType);
            }

            try
            {
                await handler.ExecuteAsync(context, command).ConfigureAwait(false);
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
