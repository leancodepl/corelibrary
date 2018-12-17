using System.Threading.Tasks;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CommandValidatorWrapper<TAppContext, TCommand> : ICommandValidatorWrapper
        where TCommand : ICommand
    {
        private readonly ICommandValidator<TAppContext, TCommand> validator;

        public CommandValidatorWrapper(
            ICommandValidator<TAppContext, TCommand> validator)
        {
            this.validator = validator;
        }

        public Task<ValidationResult> ValidateAsync(
            object appContext, ICommand command)
        {
            return validator.ValidateAsync(
                (TAppContext)appContext, (TCommand)command);
        }
    }
}
