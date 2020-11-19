using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using FValidationResult = FluentValidation.Results.ValidationResult;

namespace LeanCode.CQRS.Validation.Fluent.Testing
{
    public class OverrideContextValidator<T> : IValidator<T>
    {
        private readonly IValidator<T> inner;

        public CascadeMode CascadeMode
        {
            get => inner.CascadeMode;
            set => inner.CascadeMode = value;
        }

        public IComponentContext Components
        {
            get => (IComponentContext)BaseContext.RootContextData[ValidationContextExtensions.ComponentContextKey];
            set => BaseContext.RootContextData[ValidationContextExtensions.ComponentContextKey] = value;
        }

        public object AppContext
        {
            get => BaseContext.RootContextData[ValidationContextExtensions.AppContextKey];
            set => BaseContext.RootContextData[ValidationContextExtensions.AppContextKey] = value;
        }

        public ValidationContext<T> BaseContext { get; }

        public OverrideContextValidator(IValidator<T> inner, ValidationContext<T> context)
        {
            this.inner = inner;
            BaseContext = context;
        }

        public OverrideContextValidator(IValidator<T> inner)
        {
            this.inner = inner;
            BaseContext = new ValidationContext<T>(default!);
        }

        public bool CanValidateInstancesOfType(Type type) => inner.CanValidateInstancesOfType(type);

        public IValidatorDescriptor CreateDescriptor() => inner.CreateDescriptor();

        public FValidationResult Validate(T instance)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: instance);
            // sync-over-async but this immensely simplifies tests, tests should be fixed as we go
            return inner.ValidateAsync(clone).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public FValidationResult Validate(object instance)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: instance);
            return inner.ValidateAsync(clone).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public FValidationResult Validate(IValidationContext context)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: context.InstanceToValidate);
            return inner.ValidateAsync(clone).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task<FValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: instance);
            return inner.ValidateAsync(clone, cancellation);
        }

        public Task<FValidationResult> ValidateAsync(object instance, CancellationToken cancellation = default)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: instance);
            return inner.ValidateAsync(clone, cancellation);
        }

        public Task<FValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
        {
            var clone = BaseContext.CloneForChildValidator(instanceToValidate: context.InstanceToValidate);
            return inner.ValidateAsync(clone, cancellation);
        }
    }
}
