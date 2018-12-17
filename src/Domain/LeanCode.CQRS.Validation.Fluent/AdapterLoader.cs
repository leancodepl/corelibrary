using System.Threading.Tasks;
using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    class AdapterLoader<TAppContext, TContext, TCommand> : ICommandValidator<TAppContext, TCommand>
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
        public Task<ValidationResult> ValidateAsync(TAppContext appContext, TCommand command)
        {
            if (!(adapter is null))
            {
                return adapter.ValidateAsync(appContext, command);
            }
            else
            {
                return NoError;
            }
        }
    }
}
