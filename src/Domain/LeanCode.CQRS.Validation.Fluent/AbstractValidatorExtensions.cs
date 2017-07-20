using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;

namespace LeanCode.CQRS.Validation.Fluent
{
    public static class AbstractValidatorExtensions
    {
        public static IRuleBuilderInitial<T, TValue> RuleFor<T, TProperty, TValue>(
            this AbstractValidator<T> validator,
            Expression<Func<T, TProperty>> expression,
            Func<ValidationContext, TProperty, TValue> realValueAccessor)
        {
            var member = expression.GetMember();
            var compiled = member == null || ValidatorOptions.DisableAccessorCache ? expression.Compile() : AccessorCache<T>.GetCachedAccessor(member, expression);

            var rule = new ContextualPropertyRule(member,
                compiled.CoerceToNonGeneric(),
                (ctx, arg) => realValueAccessor(ctx, (TProperty)arg),
                expression,
                () => validator.CascadeMode,
                typeof(TValue),
                typeof(T));

            validator.AddRule(rule);
            return new RuleBuilder<T, TValue>(rule);
        }

        public static IRuleBuilderInitial<T, TValue> RuleForAsync<T, TProperty, TValue>(
            this AbstractValidator<T> validator,
            Expression<Func<T, TProperty>> expression,
            Func<ValidationContext, TProperty, Task<TValue>> realValueAccessor)
        {
            var member = expression.GetMember();
            var compiled = member == null || ValidatorOptions.DisableAccessorCache ? expression.Compile() : AccessorCache<T>.GetCachedAccessor(member, expression);

            var rule = new AsyncContextualPropertyRule(member,
                compiled.CoerceToNonGeneric(),
                (ctx, arg) => realValueAccessor(ctx, (TProperty)arg).ContinueWith(t => (object)t.Result),
                expression,
                () => validator.CascadeMode,
                typeof(TValue),
                typeof(T));

            validator.AddRule(rule);
            return new RuleBuilder<T, TValue>(rule);
        }
    }
}
