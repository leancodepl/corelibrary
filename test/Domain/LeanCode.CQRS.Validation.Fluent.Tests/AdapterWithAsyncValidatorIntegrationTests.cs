using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using FluentValidation;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests
{
    public class AdapterWithAsyncValidatorIntegrationTests
    {
        private readonly FluentValidationCommandValidatorAdapter<Context, Command> adapter;

        public AdapterWithAsyncValidatorIntegrationTests()
        {
            adapter = new FluentValidationCommandValidatorAdapter<Context, Command>(
                new Validator(),
                new ComponentContext());
        }

        [Fact]
        public async Task Invokes_async_validation()
        {
            // Will throw on sync invocation
            await adapter.ValidateAsync(new Context(), new Command());
        }

        [Fact]
        public async Task Correctly_returns_successful_result_when_data_passes()
        {
            var res = await adapter.ValidateAsync(
                new Context(),
                new Command { Data = Validator.MinValue });

            Assert.True(res.IsValid);
        }

        [Fact]
        public async Task Correctly_maps_validation_result()
        {
            var res = await adapter.ValidateAsync(
                new Context(),
                new Command { Data = Validator.MinValue - 1 });

            Assert.False(res.IsValid);
            var err = Assert.Single(res.Errors);
            Assert.Equal(nameof(Command.Data), err.PropertyName);
            Assert.Equal(Validator.ErrorCode, err.ErrorCode);
            Assert.Equal(Validator.ErrorMessage, err.ErrorMessage);
        }

        private class Validator : ContextualValidator<Command>
        {
            public const int MinValue = 5;
            public const int ErrorCode = 1;
            public const string ErrorMessage = "Custom message";

            public Validator()
            {
                RuleForAsync(c => c.Data, (ctx, v) => Task.FromResult(v))
                    .GreaterThanOrEqualTo(MinValue)
                        .WithCode(ErrorCode)
                        .WithMessage(ErrorMessage);
            }
        }

        private class Context
        { }

        private class Command : ICommand
        {
            public int Data { get; set; }
        }

        private class ComponentContext : IComponentContext
        {
            public IComponentRegistry ComponentRegistry => null;
            public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters) => null;
        }
    }
}
