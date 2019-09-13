using System.Threading.Tasks;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Wrappers
{
    internal class CommandValidatorWrapper<TAppContext, TCommand> : ICommandValidatorWrapper
        where TAppContext : notnull
        where TCommand : ICommand
    {
        private readonly ICommandValidator<TAppContext, TCommand> validator;

        public CommandValidatorWrapper(ICommandValidator<TAppContext, TCommand> validator)
        {
            this.validator = validator;
        }

        public Task<ValidationResult> ValidateAsync(object appContext, ICommand command) =>
            validator.ValidateAsync((TAppContext)appContext, (TCommand)command);
    }
}
