using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultCommandExecutor>();

        private readonly ICommandHandlerResolver commandHandlerResolver;
        private readonly IAuthorizer authorizer;
        private readonly ICommandValidatorResolver commandValidatorResolver;

        public DefaultCommandExecutor(ICommandHandlerResolver commandHandlerResolver,
            IAuthorizer authorizer,
            ICommandValidatorResolver commandValidatorResolver)
        {
            this.commandHandlerResolver = commandHandlerResolver;
            this.authorizer = authorizer;
            this.commandValidatorResolver = commandValidatorResolver;
        }

        public async Task<CommandResult> RunAsync<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            logger.Verbose("Executing command {@Command}", command);

            AuthorizeCommand(command);
            var failure = await ValidateCommand(command).ConfigureAwait(false);
            if (failure != null)
            {
                return failure;
            }
            return await RunCommand(command).ConfigureAwait(false);
        }

        private void AuthorizeCommand<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            if (!authorizer.CheckIfAuthorized(command))
            {
                logger.Warning("Command {@Command} not authorized", command);
                throw new InsufficientPermissionException($"User not authorized for {command.GetType()}");
            }
        }

        private async Task<CommandResult> ValidateCommand<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var commandValidator = commandValidatorResolver.GetValidator<TCommand>();
            if (commandValidator != null)
            {
                var result = await commandValidator.ValidateAsync(command).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    logger.Information("Command {@Command} is not valid", command);
                    return CommandResult.NotValid(result);
                }
            }
            return null;
        }

        private async Task<CommandResult> RunCommand<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var handler = commandHandlerResolver.FindCommandHandler<TCommand>();
            if (handler == null)
            {
                logger.Fatal("Cannot find a handler for the command {@Command}", command);
                throw new CommandHandlerNotFoundException(typeof(TCommand));
            }

            try
            {
                await handler.ExecuteAsync(command).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Cannot execute command {@Command} because of internal error", command);
                throw;
            }
            logger.Information("Command {@Command} executed successfully", command);
            return CommandResult.Success();
        }
    }
}
