using FluentValidation;
using FluentValidation.Results;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Scoped.Tests;

public class AdapterWithAsyncValidatorIntegrationTests
{
    private readonly FluentValidationCommandValidatorAdapter<Command> adapter;

    private static HttpContext MockHttpContext() => Substitute.For<HttpContext>();

    public AdapterWithAsyncValidatorIntegrationTests()
    {
        adapter = new FluentValidationCommandValidatorAdapter<Command>(new Validator());
    }

    [Fact]
    public async Task Invokes_async_validation()
    {
        // Will throw on sync invocation
        await adapter.ValidateAsync(MockHttpContext(), new Command());
    }

    [Fact]
    public async Task Correctly_returns_successful_result_when_data_passes()
    {
        var res = await adapter.ValidateAsync(MockHttpContext(), new Command { Data = Validator.MinValue });

        Assert.True(res.IsValid);
    }

    [Fact]
    public async Task Correctly_maps_validation_result()
    {
        var res = await adapter.ValidateAsync(MockHttpContext(), new Command { Data = Validator.MinValue - 1 });

        Assert.False(res.IsValid);
        var err = Assert.Single(res.Errors);
        Assert.Equal(nameof(Command.Data), err.PropertyName);
        Assert.Equal(Validator.ErrorCode, err.ErrorCode);
        Assert.Equal(Validator.ErrorMessage, err.ErrorMessage);
    }

    [Fact]
    public async Task Passes_http_context_to_validation_context()
    {
        var validator = Substitute.For<IValidator>();
        var httpContext = MockHttpContext();
        var command = new Command();
        var adapter = new FluentValidationCommandValidatorAdapter<Command>(validator);

        HttpContext interceptedHttpContext = null;
        _ = validator
            .ValidateAsync(
                Arg.Do((IValidationContext ctx) => interceptedHttpContext = ctx.HttpContext()),
                Arg.Any<CancellationToken>()
            )
            .Returns(new ValidationResult());

        await adapter.ValidateAsync(httpContext, command);

        Assert.Equal(httpContext, interceptedHttpContext);
    }

    private class Validator : AbstractValidator<Command>
    {
        public const int MinValue = 5;
        public const int ErrorCode = 1;
        public const string ErrorMessage = "Custom message";

        public Validator()
        {
            RuleFor(c => c.Data).GreaterThanOrEqualTo(MinValue).WithCode(ErrorCode).WithMessage(ErrorMessage);
        }
    }

    private class Command : ICommand
    {
        public int Data { get; set; }
    }
}
