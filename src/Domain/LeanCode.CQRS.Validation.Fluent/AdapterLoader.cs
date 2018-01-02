using System.Threading.Tasks;
using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    class AdapterLoader<TAppContext, TContext, TCommand> : ICommandValidator<TAppContext, TContext, TCommand>
        where TCommand : ICommand<TContext>
    {
        private static readonly Task<ValidationResult> NoError =
            Task.FromResult(new ValidationResult(null));

        private readonly FluentValidationCommandValidatorAdapter<TAppContext, TContext, TCommand> adapter;

        public AdapterLoader(IComponentContext ctx)
        {
            if (ctx.TryResolve<IValidator<TCommand>>(out var val))
            {
                adapter = new FluentValidationCommandValidatorAdapter<TAppContext, TContext, TCommand>(val, ctx);
            }
        }
        public Task<ValidationResult> ValidateAsync(TAppContext appContext, TContext context, TCommand command)
        {
            if (adapter != null)
            {
                return adapter.ValidateAsync(appContext, context, command);
            }
            else
            {
                return NoError;
            }
        }
    }
}
