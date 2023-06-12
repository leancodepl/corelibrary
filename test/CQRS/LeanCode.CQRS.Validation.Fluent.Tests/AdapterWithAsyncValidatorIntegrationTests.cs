using FluentAssertions;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace LeanCode.CQRS.Validation.Fluent.Tests;

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

        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Correctly_maps_validation_result()
    {
        var res = await adapter.ValidateAsync(
            MockHttpContext(),
            new Command { Data = Validator.MinValue - 1, FailCustom = true }
        );

        res.IsValid.Should().BeFalse();
        res.Errors
            .Should()
            .BeEquivalentTo(
                new ValidationError[]
                {
                    new(nameof(Command.Data), Validator.MinValueErrorMessage, Validator.MinValueErrorCode),
                    new(nameof(Command.FailCustom), Validator.CustomErrorMessage, Validator.CustomErrorCode),
                }
            );
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

        interceptedHttpContext.Should().BeSameAs(httpContext);
    }

    private class Validator : AbstractValidator<Command>
    {
        public const int MinValue = 5;

        public const int MinValueErrorCode = 1;
        public const string MinValueErrorMessage = "Min value error message";
        public const int CustomErrorCode = 2;
        public const string CustomErrorMessage = "Custom error message";

        public Validator()
        {
            RuleFor(c => c.Data)
                .GreaterThanOrEqualTo(MinValue)
                .WithCode(MinValueErrorCode)
                .WithMessage(MinValueErrorMessage);
            RuleFor(cmd => cmd).CustomAsync(CustomValidateAsync);
        }

        private Task CustomValidateAsync(Command cmd, ValidationContext<Command> ctx, CancellationToken ct)
        {
            if (cmd.FailCustom)
            {
                ctx.AddValidationError(CustomErrorMessage, CustomErrorCode, nameof(Command.FailCustom));
            }

            return Task.CompletedTask;
        }
    }

    private class Command : ICommand
    {
        public int Data { get; set; }
        public bool FailCustom { get; set; }
    }
}
