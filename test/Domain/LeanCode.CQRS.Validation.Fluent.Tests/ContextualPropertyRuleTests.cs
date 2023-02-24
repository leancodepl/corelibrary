using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests;

public class ContextualPropertyRuleTests
{
    [Fact]
    public void Passes_the_property_data_to_value()
    {
        const string Value = "Value";
        string dataPassed = null;
        object Func(IValidationContext ctx, string str)
        {
            dataPassed = str;
            return null;
        }

        var validator = new TestValidator(Func);
        validator.Validate(new SampleData { Test = Value });

        Assert.Equal(Value, dataPassed);
    }

    [Fact]
    public void Passes_the_context_to_accessor()
    {
        IValidationContext dataPassed = null;
        object Func(IValidationContext ctx2, string str)
        {
            dataPassed = ctx2;
            return null;
        }

        var validator = new TestValidator(Func);
        var ctx = new ValidationContext<SampleData>(new SampleData());
        validator.Validate(ctx);

        Assert.Equal(ctx, dataPassed);
    }

    [Fact]
    public void Passes_accessed_value_to_validators()
    {
        var obj = new object();
        object dataPassed = null;

        var validator = new TestValidator((_, _) => obj, e => (dataPassed = e) != null);
        validator.Validate(new SampleData());

        Assert.Equal(obj, dataPassed);
    }

    [Fact]
    public async Task Passes_the_property_data_to_value_async()
    {
        const string Value = "Value";
        string dataPassed = null;
        object Func(IValidationContext ctx, string str)
        {
            dataPassed = str;
            return null;
        }

        var validator = new TestValidator(Func);
        await validator.ValidateAsync(new SampleData { Test = Value });

        Assert.Equal(Value, dataPassed);
    }

    [Fact]
    public async Task Passes_the_context_to_accessor_async()
    {
        IValidationContext dataPassed = null;
        object Func(IValidationContext ctx2, string str)
        {
            dataPassed = ctx2;
            return null;
        }

        var validator = new TestValidator(Func);
        var ctx = new ValidationContext<SampleData>(new SampleData());
        await validator.ValidateAsync(ctx);

        Assert.Equal(ctx, dataPassed);
    }

    [Fact]
    public async Task Passes_accessed_value_to_validators_async()
    {
        var obj = new object();
        object dataPassed = null;

        var validator = new TestValidator((_, _) => obj, e => (dataPassed = e) != null);
        await validator.ValidateAsync(new SampleData());

        Assert.Equal(obj, dataPassed);
    }

    [Fact]
    public void The_accessor_is_called_only_once_for_multiple_validation_rules_sync_case()
    {
        var calledCount = 0;
        object Accessor(IValidationContext ctx, object data)
        {
            return Interlocked.Increment(ref calledCount);
        }

        var validator = new MultiValidator(Accessor);
        validator.Validate(new SampleData());

        Assert.Equal(1, calledCount);
    }

    [Fact]
    public async Task The_accessor_is_called_only_once_for_multiple_validation_rules_async_case()
    {
        var calledCount = 0;
        object Accessor(IValidationContext ctx, object data)
        {
            return Interlocked.Increment(ref calledCount);
        }

        var validator = new MultiValidator(Accessor);
        await validator.ValidateAsync(new SampleData());

        Assert.Equal(1, calledCount);
    }

    private class TestValidator : ContextualValidator<SampleData>
    {
        public TestValidator(Func<IValidationContext, string, object> accessor, Func<object, bool> must = null)
        {
            RuleFor(d => d.Test, accessor).Must(must ?? (e => true));
        }
    }

    private class MultiValidator : ContextualValidator<SampleData>
    {
        public MultiValidator(Func<IValidationContext, string, object> accessor)
        {
            RuleFor(d => d.Test, accessor).Must(_ => true).Must(_ => true);
        }
    }

    private class SampleData
    {
        public string Test { get; set; }
    }
}
