using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.Components;
using LeanCode.Contracts;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Scoped.Tests;

public class ModuleTests
{
    private readonly IContainer container;

    public ModuleTests()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(new FluentValidationModule(TypesCatalog.Of<ModuleTests>()));
        builder.RegisterInstance(new ExternalDependency());
        container = builder.Build();
    }

    [Fact]
    public async Task Resolves_custom_ContextualValidatorWithInt_as_ICommandValidator()
    {
        var validator = container.Resolve<ICommandValidator<object, CustomCommandWithInt>>();

        var res = await validator.ValidateAsync(new object(), new CustomCommandWithInt { Field = 0 });

        var err = Assert.Single(res.Errors);
        Assert.Equal(10, err.ErrorCode);
    }

    [Fact]
    public async Task Resolves_command_validator_where_there_is_no_validator()
    {
        var validator = container.Resolve<ICommandValidator<object, CustomCommandWithoutCustomValidator>>();

        var res = await validator.ValidateAsync(new object(), new CustomCommandWithoutCustomValidator { Field = 0 });

        Assert.NotNull(validator);
        Assert.Empty(res.Errors);
    }

    [Fact]
    public async Task Resolves_command_validator_taking_ExternalDependency_as_ctor_parameterAsync()
    {
        var validator = container.Resolve<ICommandValidator<object, CustomCommandWithString>>();

        var res = await validator.ValidateAsync(new object(), new CustomCommandWithString { Field = "test" });

        var err = Assert.Single(res.Errors);
        Assert.Equal(13, err.ErrorCode);
    }

    [Fact]
    public async Task Resolves_command_validator_taking_AppCtx_as_a_ctor_parameterAsync()
    {
        var appContext = new AppCtx();

        var validator = container.Resolve<ICommandValidator<AppCtx, CustomCommandWithFloat>>();

        var res = await validator.ValidateAsync(appContext, new CustomCommandWithFloat { Field = 1.3f });

        Assert.NotNull(validator);
        var err = Assert.Single(res.Errors);
        Assert.Equal("test", err.ErrorMessage);
    }

    [Fact]
    public void Ensure_that_the_same_validator_resolved_in_two_diffrent_scopes_is_not_the_same_instance()
    {
        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();

        var validator1 = scope1.Resolve<ICommandValidator<object, CustomCommandWithInt>>();
        var validator2 = scope2.Resolve<ICommandValidator<object, CustomCommandWithInt>>();

        Assert.NotSame(validator1, validator2);
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
    public int Number = 13;
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
    public string Message = "test";
}

public class CustomCommandWithoutCustomValidator : ICommand
{
    public int Field { get; set; }
}
