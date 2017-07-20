using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using FluentValidation.Internal;

using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace LeanCode.CQRS.Validation.Fluent
{
    class FluentValidationCommandValidatorAdapter<TCommand> : ICommandValidator<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator fluentValidator;
        private readonly IComponentContext componentContext;

        public FluentValidationCommandValidatorAdapter(IValidator fluentValidator, IComponentContext componentContext)
        {
            this.fluentValidator = fluentValidator;
            this.componentContext = componentContext;
        }

        public async Task<ValidationResult> ValidateAsync(TCommand command)
        {
            var ctx = PrepareContext(command);

            var fluentValidationResult = await fluentValidator.ValidateAsync(ctx).ConfigureAwait(false);
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

        private ValidationContext<TCommand> PrepareContext(TCommand command)
        {
            var ctx = new ValidationContext<TCommand>(command,
                new PropertyChain(),
                ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory());
            ctx.RootContextData[ValidationContextExtensions.ComponentContextKey] = this.componentContext;
            return ctx;
        }
    }
}
