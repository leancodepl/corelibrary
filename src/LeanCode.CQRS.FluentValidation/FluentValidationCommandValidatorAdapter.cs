using System.Linq;
using LeanCode.CQRS.Validation;
using FluentValidation;

using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace LeanCode.CQRS.FluentValidation
{
    class FluentValidationCommandValidatorAdapter<TCommand> : ICommandValidator<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator fluentValidator;

        public FluentValidationCommandValidatorAdapter(IValidator fluentValidator)
        {
            this.fluentValidator = fluentValidator;
        }

        public ValidationResult Validate(TCommand command)
        {
            var fluentValidationResult = fluentValidator.Validate(command);
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
                state?.ErrorCode ?? 0,
                failure.AttemptedValue
            );
        }
    }
}
