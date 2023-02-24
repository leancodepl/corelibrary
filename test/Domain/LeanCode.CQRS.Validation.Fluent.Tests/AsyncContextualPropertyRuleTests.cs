using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests;

public class AsyncContextualPropertyRuleTests
{
    [Fact]
    public async Task Passes_the_property_data_to_value()
    {
        const string Value = "Value";
        string dataPassed = null;
        Task<object> Accessor(IValidationContext ctx, string str)
        {
            dataPassed = str;
            return Task.FromResult<object>(null);
        }

        var validator = new TestValidator(Accessor);
        await validator.ValidateAsync(new SampleData { Test = Value });

        Assert.Equal(Value, dataPassed);
    }

    [Fact]
    public async Task Passes_the_context_to_accessor()
    {
        IValidationContext dataPassed = null;
        Task<object> Accessor(IValidationContext ctx2, string str)
        {
            dataPassed = ctx2;
            return Task.FromResult<object>(null);
        }

        var validator = new TestValidator(Accessor);
        var ctx = new ValidationContext<SampleData>(new SampleData());
        await validator.ValidateAsync(ctx);

        Assert.Equal(ctx, dataPassed);
    }

    [Fact]
    public async Task Passes_accessed_value_to_validators()
    {
        var obj = new object();
        object dataPassed = null;

        var validator = new TestValidator((_, _) => Task.FromResult(obj), e => (dataPassed = e) != null);
        await validator.ValidateAsync(new SampleData());

        Assert.Equal(obj, dataPassed);
    }

    [Fact]
    public async Task The_accessor_is_called_only_once_for_multiple_validation_rules()
    {
        var calledCount = 0;
        Task<object> Accessor(IValidationContext ctx, object data)
        {
            return Task.FromResult((object)Interlocked.Increment(ref calledCount));
        }

        var validator = new MultiValidator(Accessor);
        await validator.ValidateAsync(new SampleData());

        Assert.Equal(1, calledCount);
    }

    [Fact]
    public void Synchronous_validation_is_forbidden_for_async_rules()
    {
        var obj = new object();
        object dataPassed = null;

        var validator = new TestValidator((_, _) => Task.FromResult(obj), e => (dataPassed = e) != null);
        Assert.Throws<NotSupportedException>(() => validator.Validate(new SampleData()));
    }

    private class TestValidator : ContextualValidator<SampleData>
    {
        public TestValidator(Func<IValidationContext, string, Task<object>> accessor, Func<object, bool> must = null)
        {
            RuleForAsync(d => d.Test, accessor).Must(must ?? (e => true));
        }
    }

    private class MultiValidator : ContextualValidator<SampleData>
    {
        public MultiValidator(Func<IValidationContext, string, Task<object>> accessor)
        {
            RuleForAsync(d => d.Test, accessor).Must(e => true).Must(e => true);
        }
    }

    private class SampleData
    {
        public string Test { get; set; }
    }
}
