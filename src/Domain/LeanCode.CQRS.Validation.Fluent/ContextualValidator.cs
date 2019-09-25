using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;

namespace LeanCode.CQRS.Validation.Fluent
{
    public class ContextualValidator<T> : AbstractValidator<T>
    {
        public IRuleBuilderInitial<T, TValue> RuleFor<TProperty, TValue>(
            Expression<Func<T, TProperty>> expression,
            Func<ValidationContext, TProperty, TValue> realValueAccessor)
        {
            var member = expression.GetMember();

            var compiled = member is null || ValidatorOptions.DisableAccessorCache
                ? expression.Compile()
                : AccessorCache<T>.GetCachedAccessor(member, expression);

            var rule = new ContextualPropertyRule(
                member,
                compiled.CoerceToNonGeneric(),
                (ctx, arg) => realValueAccessor(ctx, (TProperty)arg),
                expression,
                () => CascadeMode,
                typeof(TValue),
                typeof(T));

            AddRule(rule);

            return new RuleBuilder<T, TValue>(rule, this);
        }

        public IRuleBuilderInitial<T, TValue> RuleForAsync<TProperty, TValue>(
            Expression<Func<T, TProperty>> expression,
            Func<ValidationContext, TProperty, Task<TValue>> realValueAccessor)
        {
            var member = expression.GetMember();

            var compiled = member is null || ValidatorOptions.DisableAccessorCache
                ? expression.Compile()
                : AccessorCache<T>.GetCachedAccessor(member, expression);

            var rule = new AsyncContextualPropertyRule(
                member,
                compiled.CoerceToNonGeneric(),
                async (ctx, arg) => await realValueAccessor(ctx, (TProperty)arg),
                expression,
                () => CascadeMode,
                typeof(TValue),
                typeof(T));

            AddRule(rule);

            return new RuleBuilder<T, TValue>(rule, this);
        }
    }
}
