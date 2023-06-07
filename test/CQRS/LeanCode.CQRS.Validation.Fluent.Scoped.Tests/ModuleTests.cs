using FluentAssertions;
using FluentValidation;
using LeanCode.Components;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Scoped.Tests;

public class ModuleTests
{
    private readonly IServiceProvider serviceProvider;

    private static HttpContext MockHttpContext() => Substitute.For<HttpContext>();

    public ModuleTests()
    {
        var builder = new ServiceCollection();
        builder.AddFluentValidation(TypesCatalog.Of<ModuleTests>());
        builder.AddSingleton(new ExternalDependency());
        serviceProvider = builder.BuildServiceProvider();
    }

    [Fact]
    public async Task Resolves_custom_ContextualValidatorWithInt_as_ICommandValidator()
    {
        var validator = serviceProvider.GetRequiredService<ICommandValidator<CustomCommandWithInt>>();

        var res = await validator.ValidateAsync(MockHttpContext(), new CustomCommandWithInt { Field = 0 });

        res.Errors.Should().ContainSingle().Which.ErrorCode.Should().Be(10);
    }

    [Fact]
    public async Task Resolves_command_validator_where_there_is_no_validator()
    {
        var validator = serviceProvider.GetRequiredService<ICommandValidator<CustomCommandWithoutCustomValidator>>();

        var res = await validator.ValidateAsync(
            MockHttpContext(),
            new CustomCommandWithoutCustomValidator { Field = 0 }
        );

        validator.Should().NotBeNull();
        res.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Resolves_command_validator_taking_ExternalDependency_as_ctor_parameterAsync()
    {
        var validator = serviceProvider.GetRequiredService<ICommandValidator<CustomCommandWithString>>();

        var res = await validator.ValidateAsync(MockHttpContext(), new CustomCommandWithString { Field = "test" });

        res.Errors.Should().ContainSingle().Which.ErrorCode.Should().Be(13);
    }

    [Fact]
    public void Ensure_that_the_same_validator_resolved_in_two_different_scopes_is_not_the_same_instance()
    {
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var validator1 = scope1.ServiceProvider.GetRequiredService<ICommandValidator<CustomCommandWithInt>>();
        var validator2 = scope2.ServiceProvider.GetRequiredService<ICommandValidator<CustomCommandWithInt>>();

        validator1.Should().NotBeSameAs(validator2);
    }
}

public class CustomCommandWithInt : ICommand
{
    public int Field { get; set; }
}

public class CustomValidator : AbstractValidator<CustomCommandWithInt>
{
    public CustomValidator()
    {
        RuleFor(c => c.Field).GreaterThan(5).WithCode(10);
    }
}

public class CustomCommandWithString : ICommand
{
    public string Field { get; set; }
}

public class CustomValidatorTakingExternalDependency : AbstractValidator<CustomCommandWithString>
{
    public CustomValidatorTakingExternalDependency(ExternalDependency dependency)
    {
        RuleFor(c => c.Field).Empty().WithCode(dependency.Number);
    }
}

public class ExternalDependency
{
    public int Number { get; set; } = 13;
}

public class CustomCommandWithFloat : ICommand
{
    public float Field { get; set; }
}

public class CustomValidatorTakingTAppContext : AbstractValidator<CustomCommandWithFloat>
{
    public CustomValidatorTakingTAppContext(AppCtx appContext)
    {
        RuleFor(c => c.Field).GreaterThan(5).WithMessage(appContext.Message);
    }
}

public class AppCtx
{
    public string Message { get; set; } = "test";
}

public class CustomCommandWithoutCustomValidator : ICommand
{
    public int Field { get; set; }
}
