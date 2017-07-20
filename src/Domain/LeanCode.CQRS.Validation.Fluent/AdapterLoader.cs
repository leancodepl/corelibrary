using System.Threading.Tasks;
using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    class AdapterLoader<TCommand> : ICommandValidator<TCommand>
        where TCommand : ICommand
    {
        private static readonly Task<ValidationResult> NoError =
            Task.FromResult(new ValidationResult(null));

        private readonly FluentValidationCommandValidatorAdapter<TCommand> adapter;

        public AdapterLoader(IComponentContext ctx)
        {
            if (ctx.TryResolve<IValidator<TCommand>>(out var val))
            {
                adapter = new FluentValidationCommandValidatorAdapter<TCommand>(val, ctx);
            }
        }

        public Task<ValidationResult> ValidateAsync(TCommand command)
        {
            if (adapter != null)
            {
                return adapter.ValidateAsync(command);
            }
            else
            {
                return NoError;
            }
        }
    }
}
