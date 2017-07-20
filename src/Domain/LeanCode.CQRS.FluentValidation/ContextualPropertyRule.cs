using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace LeanCode.CQRS.FluentValidation
{
    class ContextualPropertyRule : PropertyRule
    {
        private readonly Func<ValidationContext, object, object> realValueFunc;

        public ContextualPropertyRule(MemberInfo member, Func<object, object> propertyFunc,
            Func<ValidationContext, object, object> realValueFunc,
            LambdaExpression expression, Func<CascadeMode> cascadeModeThunk,
            Type typeToValidate, Type containerType)
            : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
        {
            this.realValueFunc = realValueFunc;
        }

        protected override IEnumerable<ValidationFailure> InvokePropertyValidator(ValidationContext context, IPropertyValidator validator, string propertyName)
        {
            var propContext = CreatePropertyContext(context, propertyName);
            return validator.Validate(propContext);
        }

        protected override Task<IEnumerable<ValidationFailure>> InvokePropertyValidatorAsync(ValidationContext context, IPropertyValidator validator, string propertyName, CancellationToken cancellation)
        {
            var propContext = CreatePropertyContext(context, propertyName);
            return validator.ValidateAsync(propContext, cancellation);
        }

        private PropertyValidatorContext CreatePropertyContext(ValidationContext context, string propertyName)
        {
            var propValue = PropertyFunc(context.InstanceToValidate);
            var realValue = realValueFunc(context, propValue);
            var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);
            return propContext;
        }
    }

    class AsyncContextualPropertyRule : PropertyRule
    {
        private readonly Func<ValidationContext, object, Task<object>> realValueFunc;

        public AsyncContextualPropertyRule(MemberInfo member, Func<object, object> propertyFunc,
            Func<ValidationContext, object, Task<object>> realValueFunc,
            LambdaExpression expression, Func<CascadeMode> cascadeModeThunk,
            Type typeToValidate, Type containerType)
            : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
        {
            this.realValueFunc = realValueFunc;
        }

        protected override IEnumerable<ValidationFailure> InvokePropertyValidator(ValidationContext context, IPropertyValidator validator, string propertyName)
        {
            var propContext = CreatePropertyContext(context, propertyName).Result;
            return validator.Validate(propContext);
        }

        protected override async Task<IEnumerable<ValidationFailure>> InvokePropertyValidatorAsync(ValidationContext context, IPropertyValidator validator, string propertyName, CancellationToken cancellation)
        {
            var propContext = await CreatePropertyContext(context, propertyName).ConfigureAwait(false);
            return await validator.ValidateAsync(propContext, cancellation).ConfigureAwait(false);
        }

        private async Task<PropertyValidatorContext> CreatePropertyContext(ValidationContext context, string propertyName)
        {
            var propValue = PropertyFunc(context.InstanceToValidate);
            var realValue = await realValueFunc(context, propValue).ConfigureAwait(false);
            var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);
            return propContext;
        }
    }
}
