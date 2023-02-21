using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent;

public static class FluentValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        int code)
    {
        return rule.WithState(_ => new FluentValidatorErrorState(code));
    }
}
