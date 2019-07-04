using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests
{
    public class AsyncContextualPropertyRuleTests
    {
        [Fact]
        public async Task Passes_the_property_data_to_value()
        {
            const string value = "Value";
            string dataPassed = null;
            Task<object> accessor(ValidationContext ctx, string str)
            {
                dataPassed = str;
                return Task.FromResult<object>(null);
            }

            var validator = new TestValidator(accessor);
            await validator.ValidateAsync(new SampleData { Test = value });

            Assert.Equal(value, dataPassed);
        }

        [Fact]
        public async Task Passes_the_context_to_accessor()
        {
            ValidationContext dataPassed = null;
            Task<object> accessor(ValidationContext ctx2, string str)
            {
                dataPassed = ctx2;
                return Task.FromResult<object>(null);
            }

            var validator = new TestValidator(accessor);
            var ctx = new ValidationContext<SampleData>(new SampleData());
            await validator.ValidateAsync(ctx);

            Assert.Equal(ctx, dataPassed);
        }

        [Fact]
        public async Task Passes_accessed_value_to_validators()
        {
            var obj = new object();
            object dataPassed = null;

            var validator = new TestValidator((_, __) => Task.FromResult(obj), e => (dataPassed = e) != null);
            await validator.ValidateAsync(new SampleData());

            Assert.Equal(obj, dataPassed);
        }

        [Fact]
        public async Task The_accessor_is_called_only_once_for_multiple_validation_rules()
        {
            int calledCount = 0;
            Task<object> accessor(ValidationContext _, object data)
            {
                return Task.FromResult((object)Interlocked.Increment(ref calledCount));
            }

            var validator = new MultiValidator(accessor);
            await validator.ValidateAsync(new SampleData());

            Assert.Equal(1, calledCount);
        }

        [Fact]
        public void Synchronous_validation_is_forbidden_for_async_rules()
        {
            var obj = new object();
            object dataPassed = null;

            var validator = new TestValidator((_, __) => Task.FromResult(obj), e => (dataPassed = e) != null);
            Assert.Throws<NotSupportedException>(() => validator.Validate(new SampleData()));
        }

        class TestValidator : ContextualValidator<SampleData>
        {
            public TestValidator(Func<ValidationContext, string, Task<object>> accessor, Func<object, bool> must = null)
            {
                RuleForAsync(d => d.Test, accessor).Must(must ?? (e => true));
            }
        }

        class MultiValidator : ContextualValidator<SampleData>
        {
            public MultiValidator(Func<ValidationContext, string, Task<object>> accessor)
            {
                RuleForAsync(d => d.Test, accessor)
                    .Must(e => true)
                    .Must(e => true);
            }
        }

        class SampleData
        {
            public string Test { get; set; }
        }
    }
}
