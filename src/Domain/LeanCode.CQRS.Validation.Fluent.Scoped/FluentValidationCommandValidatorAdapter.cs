using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;

namespace LeanCode.CQRS.Validation.Fluent.Scoped;

public class FluentValidationCommandValidatorAdapter<TAppContext, TCommand> : ICommandValidator<TAppContext, TCommand>
    where TCommand : ICommand
    where TAppContext : notnull
{
    private readonly IValidator fluentValidator;

    public FluentValidationCommandValidatorAdapter(IValidator fluentValidator)
    {
        this.fluentValidator = fluentValidator;
    }

    public async Task<Contracts.Validation.ValidationResult> ValidateAsync(TAppContext appContext, TCommand command)
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
