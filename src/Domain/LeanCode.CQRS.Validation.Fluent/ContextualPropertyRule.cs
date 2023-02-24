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

namespace LeanCode.CQRS.Validation.Fluent;

internal class ContextualPropertyRule : PropertyRule
{
    private const string InstanceUnderValidationKey = "InstanceUnderValidation";

    private readonly Func<IValidationContext, object, object?> realValueFunc;

    public ContextualPropertyRule(
        MemberInfo? member,
        Func<object, object>? propertyFunc,
        Func<IValidationContext, object, object?> realValueFunc,
        LambdaExpression? expression,
        Func<CascadeMode>? cascadeModeThunk,
        Type? typeToValidate,
        Type? containerType
    )
        : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
    {
        this.realValueFunc = realValueFunc;
    }

    public override IEnumerable<ValidationFailure> Validate(IValidationContext context)
    {
        context.RootContextData[InstanceUnderValidationKey] = GetRealValue(context);

        return base.Validate(context);
    }

    public override Task<IEnumerable<ValidationFailure>> ValidateAsync(
        IValidationContext context,
        CancellationToken cancellation
    )
    {
        context.RootContextData[InstanceUnderValidationKey] = GetRealValue(context);

        return base.ValidateAsync(context, cancellation);
    }

    protected override IEnumerable<ValidationFailure> InvokePropertyValidator(
        IValidationContext context,
        IPropertyValidator validator,
        string propertyName
    )
    {
        var propContext = CreatePropertyContext(context, propertyName);

        return validator.Validate(propContext);
    }

    protected override Task<IEnumerable<ValidationFailure>> InvokePropertyValidatorAsync(
        IValidationContext context,
        IPropertyValidator validator,
        string propertyName,
        CancellationToken cancellation
    )
    {
        var propContext = CreatePropertyContext(context, propertyName);

        return validator.ValidateAsync(propContext, cancellation);
    }

    private PropertyValidatorContext CreatePropertyContext(IValidationContext context, string propertyName)
    {
        var realValue = context.RootContextData[InstanceUnderValidationKey];
        var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);

        return propContext;
    }

    private object? GetRealValue(IValidationContext context)
    {
        var propValue = PropertyFunc(context.InstanceToValidate);

        return realValueFunc(context, propValue);
    }
}

internal class AsyncContextualPropertyRule : PropertyRule
{
    private const string InstanceUnderValidationKey = "InstanceUnderValidation";

    private readonly Func<IValidationContext, object, Task<object?>> realValueFunc;

    public AsyncContextualPropertyRule(
        MemberInfo? member,
        Func<object, object>? propertyFunc,
        Func<IValidationContext, object, Task<object?>> realValueFunc,
        LambdaExpression? expression,
        Func<CascadeMode>? cascadeModeThunk,
        Type? typeToValidate,
        Type? containerType
    )
        : base(member, propertyFunc, expression, cascadeModeThunk, typeToValidate, containerType)
    {
        this.realValueFunc = realValueFunc;
    }

    public override IEnumerable<ValidationFailure> Validate(IValidationContext context)
    {
        throw new NotSupportedException("Cannot execute async validator in sync context.");
    }

    public override async Task<IEnumerable<ValidationFailure>> ValidateAsync(
        IValidationContext context,
        CancellationToken cancellation
    )
    {
        context.RootContextData[InstanceUnderValidationKey] = await GetRealValueAsync(context);

        return await base.ValidateAsync(context, cancellation);
    }

    protected override IEnumerable<ValidationFailure> InvokePropertyValidator(
        IValidationContext context,
        IPropertyValidator validator,
        string propertyName
    )
    {
        var propContext = CreatePropertyContext(context, propertyName);

        return validator.Validate(propContext);
    }

    protected override async Task<IEnumerable<ValidationFailure>> InvokePropertyValidatorAsync(
        IValidationContext context,
        IPropertyValidator validator,
        string propertyName,
        CancellationToken cancellation
    )
    {
        var propContext = CreatePropertyContext(context, propertyName);

        return await validator.ValidateAsync(propContext, cancellation);
    }

    private PropertyValidatorContext CreatePropertyContext(IValidationContext context, string propertyName)
    {
        var realValue = context.RootContextData[InstanceUnderValidationKey];
        var propContext = new PropertyValidatorContext(context, this, propertyName, realValue);

        return propContext;
    }

    private async Task<object?> GetRealValueAsync(IValidationContext context)
    {
        var propValue = PropertyFunc(context.InstanceToValidate);

        return await realValueFunc(context, propValue);
    }
}
