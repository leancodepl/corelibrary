using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Validation.Fluent;

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
        var ctx = PrepareContext(httpContext, command);

        var fluentValidationResult = await fluentValidator.ValidateAsync(ctx, httpContext.RequestAborted);

        var mappedResult = fluentValidationResult.Errors.Select(MapFluentError).ToList();

        return new(mappedResult);
    }

    private static ValidationError MapFluentError(ValidationFailure failure)
    {
        var state = failure.CustomState as FluentValidatorErrorState;

        return new ValidationError(failure.PropertyName, failure.ErrorMessage, state?.ErrorCode ?? 0);
    }

    private static ValidationContext<TCommand> PrepareContext(HttpContext httpContext, TCommand command)
    {
        return new ValidationContext<TCommand>(
            command,
            new PropertyChain(),
            ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory()
        )
        {
            RootContextData = { [ValidationContextExtensions.HttpContextKey] = httpContext, }
        };
    }
}
