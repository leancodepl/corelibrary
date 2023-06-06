using FluentValidation;
using LeanCode.Components;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.Validation.Fluent.Tests;

public class ModuleTests
{
    private readonly IServiceProvider container;

    public ModuleTests()
    {
        var builder = new ServiceCollection();
        builder.AddFluentValidation(TypesCatalog.Of<ModuleTests>());
        container = builder.BuildServiceProvider();
    }

    [Fact]
    public async Task Resolves_custom_ContextualValidator_as_ICommandValidator()
    {
        var validator = container.GetRequiredService<ICommandValidator<CustomCommand>>();

        var res = await validator.ValidateAsync(new DefaultHttpContext(), new CustomCommand { Field = 0 });

        var err = Assert.Single(res.Errors);
        Assert.Equal(10, err.ErrorCode);
    }
}

public class CustomCommand : ICommand
{
    public int Field { get; set; }
}

public class CustomValidator : ContextualValidator<CustomCommand>
{
    public CustomValidator()
    {
        RuleFor(c => c.Field).GreaterThan(5).WithCode(10);
    }
}
