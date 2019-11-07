using System.Threading.Tasks;
using LeanCode.Pipelines;

namespace LeanCode.CQRS.Execution
{
    public sealed class CommandFinalizer<TAppContext> : IPipelineFinalizer<TAppContext, ICommand, CommandResult>
        where TAppContext : IPipelineContext
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<CommandFinalizer<TAppContext>>();

        private readonly ICommandHandlerResolver<TAppContext> resolver;

        public CommandFinalizer(ICommandHandlerResolver<TAppContext> resolver)
        {
            this.resolver = resolver;
        }

        public async Task<CommandResult> ExecuteAsync(TAppContext appContext, ICommand command)
        {
            var commandType = command.GetType();
            var handler = resolver.FindCommandHandler(commandType);

            if (handler is null)
            {
                logger.Fatal("Cannot find a handler for the command {@Command}", command);

                throw new CommandHandlerNotFoundException(commandType);
            }

            await handler.ExecuteAsync(appContext, command);

            return CommandResult.Success;
        }
    }
}
