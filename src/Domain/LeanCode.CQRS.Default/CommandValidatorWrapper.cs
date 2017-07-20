using System.Threading.Tasks;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default
{
    class CommandValidatorWrapper<TCommand> : ICommandValidatorWrapper
        where TCommand : ICommand
    {
        private readonly ICommandValidator<TCommand> validator;

        public CommandValidatorWrapper(ICommandValidator<TCommand> validator)
        {
            this.validator = validator;
        }

        public Task<ValidationResult> ValidateAsync(ICommand command)
        {
            return validator.ValidateAsync((TCommand)command);
        }
    }
}
