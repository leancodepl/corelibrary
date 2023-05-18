using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Validation.Fluent.Scoped;

public class FluentValidationCommandValidatorAdapter<TCommand> : ICommandValidator<TCommand>
    where TCommand : ICommand
{
    private readonly IValidator fluentValidator;

    public FluentValidationCommandValidatorAdapter(IValidator fluentValidator)
    {
        this.fluentValidator = fluentValidator;
    }

    public async Task<Contracts.Validation.ValidationResult> ValidateAsync(HttpContext httpContext, TCommand command)
    {
        var ctx = PrepareContext(command);

        var fluentValidationResult = await fluentValidator.ValidateAsync(ctx);

        var mappedResult = fluentValidationResult.Errors.Select(MapFluentError).ToList();

        return new(mappedResult);
    }

    private ValidationError MapFluentError(ValidationFailure failure)
    {
        var state = failure.CustomState as FluentValidatorErrorState;

        return new ValidationError(failure.PropertyName, failure.ErrorMessage, state?.ErrorCode ?? 0);
    }

    private static ValidationContext<TCommand> PrepareContext(TCommand command)
    {
        return new ValidationContext<TCommand>(
            command,
            new PropertyChain(),
            ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory()
        );
    }
}
