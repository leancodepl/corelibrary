using System.Threading.Tasks;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS.Default.Wrappers
{
    class CommandValidatorWrapper<TAppContext, TContext, TCommand> : ICommandValidatorWrapper
        where TCommand : ICommand<TContext>
    {
        private readonly ICommandValidator<TAppContext, TContext, TCommand> validator;

        public CommandValidatorWrapper(
            ICommandValidator<TAppContext, TContext, TCommand> validator)
        {
            this.validator = validator;
        }

        public Task<ValidationResult> ValidateAsync(
            object appContext, object objContext, ICommand command)
        {
            return validator.ValidateAsync(
                (TAppContext)appContext, (TContext)objContext, (TCommand)command);
        }
    }
}
