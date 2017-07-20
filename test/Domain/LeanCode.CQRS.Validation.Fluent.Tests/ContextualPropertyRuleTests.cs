using System;
using System.Threading.Tasks;
using FluentValidation;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests
{
    public class ContextualPropertyRuleTests
    {
        [Fact]
        public void Passes_the_property_data_to_value()
        {
            const string value = "Value";
            string dataPassed = null;
            Func<ValidationContext, string, object> func = (ctx, str) =>
            {
                dataPassed = str;
                return null;
            };

            var validator = new TestValidator(func);
            validator.Validate(new SampleData { Test = value });

            Assert.Equal(value, dataPassed);
        }

        [Fact]
        public void Passes_the_context_to_accessor()
        {
            ValidationContext dataPassed = null;
            Func<ValidationContext, string, object> func = (ctx2, str) =>
            {
                dataPassed = ctx2;
                return null;
            };

            var validator = new TestValidator(func);
            var ctx = new ValidationContext<SampleData>(new SampleData());
            validator.Validate(ctx);

            Assert.Equal(ctx, dataPassed);
        }

        [Fact]
        public void Passes_accessed_value_to_validators()
        {
            var obj = new object();
            object dataPassed = null;

            var validator = new TestValidator((_, __) => obj, e => (dataPassed = e) != null);
            validator.Validate(new SampleData());

            Assert.Equal(obj, dataPassed);
        }

        [Fact]
        public async Task Passes_the_property_data_to_value_async()
        {
            const string value = "Value";
            string dataPassed = null;
            Func<ValidationContext, string, object> func = (ctx, str) =>
            {
                dataPassed = str;
                return null;
            };

            var validator = new TestValidator(func);
            await validator.ValidateAsync(new SampleData { Test = value });

            Assert.Equal(value, dataPassed);
        }

        [Fact]
        public async Task Passes_the_context_to_accessor_async()
        {
            ValidationContext dataPassed = null;
            Func<ValidationContext, string, object> func = (ctx2, str) =>
            {
                dataPassed = ctx2;
                return null;
            };

            var validator = new TestValidator(func);
            var ctx = new ValidationContext<SampleData>(new SampleData());
            await validator.ValidateAsync(ctx);

            Assert.Equal(ctx, dataPassed);
        }

        [Fact]
        public async Task Passes_accessed_value_to_validators_async()
        {
            var obj = new object();
            object dataPassed = null;

            var validator = new TestValidator((_, __) => obj, e => (dataPassed = e) != null);
            await validator.ValidateAsync(new SampleData());

            Assert.Equal(obj, dataPassed);
        }

        class TestValidator : AbstractValidator<SampleData>
        {
            public TestValidator(Func<ValidationContext, string, object> accessor, Func<object, bool> must = null)
            {
                this.RuleFor(d => d.Test, accessor).Must(must ?? (e => true));
            }
        }

        class SampleData
        {
            public string Test { get; set; }
        }
    }
}
