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

namespace LeanCode.CQRS.Validation.Fluent
{
    class ContextualPropertyRule : PropertyRule
    {
        private readonly SingletonValue cachedValue;

        public ContextualPropertyRule(MemberInfo member, Func<object, object> propertyFunc,
            Func<ValidationContext, object, object> realValueFunc,
            LambdaExpression expression, Func<CascadeMode> cascadeModeThunk,
            Type typeToValidate, Type containerType)
            : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
        {
            cachedValue = new SingletonValue(realValueFunc);
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
            var realValue = cachedValue.Get(context, propValue);
            var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);
            return propContext;
        }

        class SingletonValue
        {
            private readonly object lockObj = new object();

            private bool isInitialized;
            private object value;

            private readonly Func<ValidationContext, object, object> realValueFunc;

            public SingletonValue(Func<ValidationContext, object, object> realValueFunc)
            {
                this.realValueFunc = realValueFunc;
            }

            public object Get(ValidationContext context, object obj)
            {
                if (isInitialized)
                {
                    return value;
                }
                else
                {
                    lock (lockObj)
                    {
                        if (isInitialized)
                        {
                            return value;
                        }
                        else
                        {
                            value = realValueFunc(context, obj);
                            isInitialized = true;
                            return value;
                        }
                    }
                }
            }
        }
    }

    class AsyncContextualPropertyRule : PropertyRule
    {
        private readonly SingletonValue cachedValue;

        public AsyncContextualPropertyRule(MemberInfo member, Func<object, object> propertyFunc,
            Func<ValidationContext, object, Task<object>> realValueFunc,
            LambdaExpression expression, Func<CascadeMode> cascadeModeThunk,
            Type typeToValidate, Type containerType)
            : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
        {
            cachedValue = new SingletonValue(realValueFunc);
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
            var realValue = await cachedValue.GetAsync(context, propValue).ConfigureAwait(false);
            var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);
            return propContext;
        }

        class SingletonValue
        {
            private readonly SemaphoreSlim lockObj = new SemaphoreSlim(1);
            private bool isInitialized;
            private object value;

            private readonly Func<ValidationContext, object, Task<object>> realValueFunc;

            public SingletonValue(Func<ValidationContext, object, Task<object>> realValueFunc)
            {
                this.realValueFunc = realValueFunc;
            }

            public async Task<object> GetAsync(ValidationContext context, object obj)
            {
                if (isInitialized)
                {
                    return value;
                }
                else
                {
                    await lockObj.WaitAsync();
                    try
                    {
                        if (isInitialized)
                        {
                            return value;
                        }
                        else
                        {
                            value = await realValueFunc(context, obj);
                            isInitialized = true;
                            return value;
                        }
                    }
                    finally
                    {
                        lockObj.Release();
                    }
                }
            }
        }
    }
}
