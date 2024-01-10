using FluentValidation;
using FluentValidation.Results;

namespace LeanCode.CQRS.Validation.Fluent;

public static class FluentValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        int code
    )
    {
        return rule.WithState(_ => new FluentValidatorErrorState(code));
    }

    public static void AddValidationError<T>(
        this ValidationContext<T> ctx,
        string errorMessage,
        int errorCode,
        string? propertyName = null
    )
    {
        errorMessage = ctx.MessageFormatter.BuildMessage(errorMessage);
        ctx.AddFailure(
            new ValidationFailure(propertyName ?? ctx.PropertyPath, errorMessage)
            {
                CustomState = new FluentValidatorErrorState(errorCode),
            }
        );
    }
}
