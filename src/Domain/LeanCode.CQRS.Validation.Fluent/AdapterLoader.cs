using System.Threading.Tasks;
using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    class AdapterLoader<TAppContext, TCommand> : ICommandValidator<TAppContext, TCommand>
        where TCommand : ICommand
    {
        private static readonly Task<ValidationResult> NoError =
            Task.FromResult(new ValidationResult(null));

        private readonly FluentValidationCommandValidatorAdapter<TAppContext, TCommand> adapter;

        public AdapterLoader(IComponentContext ctx)
        {
            if (ctx.TryResolve<IValidator<TCommand>>(out var val))
            {
                adapter = new FluentValidationCommandValidatorAdapter<TAppContext, TCommand>(val, ctx);
            }
        }

        public Task<ValidationResult> ValidateAsync(
            TAppContext context, TCommand command)
        {
            if (adapter != null)
            {
                return adapter.ValidateAsync(context, command);
            }
            else
            {
                return NoError;
            }
        }
    }
}
