using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using FluentValidation.Internal;

using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace LeanCode.CQRS.Validation.Fluent
{
    class FluentValidationCommandValidatorAdapter<TAppContext, TContext, TCommand>
        : ICommandValidator<TAppContext, TContext, TCommand>
        where TCommand : ICommand<TContext>
    {
        private readonly IValidator fluentValidator;
        private readonly IComponentContext componentContext;

        public FluentValidationCommandValidatorAdapter(IValidator fluentValidator, IComponentContext componentContext)
        {
            this.fluentValidator = fluentValidator;
            this.componentContext = componentContext;
        }

        public async Task<ValidationResult> ValidateAsync(TAppContext appContext, TContext context, TCommand command)
        {
            var ctx = PrepareContext(appContext, context, command);

            var fluentValidationResult = await fluentValidator
                .ValidateAsync(ctx)
                .ConfigureAwait(false);
            var mappedResult = fluentValidationResult.Errors
                .Select(MapFluentError)
                .ToList();
            return new ValidationResult(mappedResult);
        }

        private ValidationError MapFluentError(ValidationFailure failure)
        {
            var state = failure.CustomState as FluentValidatorErrorState;
            return new ValidationError(
                failure.PropertyName,
                failure.ErrorMessage,
                state?.ErrorCode ?? 0
            );
        }

        private ValidationContext<TCommand> PrepareContext(TAppContext appContext, TContext context, TCommand command)
        {
            var ctx = new ValidationContext<TCommand>(command,
                new PropertyChain(),
                ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory());
            ctx.RootContextData[ValidationContextExtensions.AppContextKey] = appContext;
            ctx.RootContextData[ValidationContextExtensions.ObjectContextKey] = context;
            ctx.RootContextData[ValidationContextExtensions.ComponentContextKey] = componentContext;
            return ctx;
        }
    }
}
