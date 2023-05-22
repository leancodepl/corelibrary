using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

public class CommandValidatorResolverTests
{
    private readonly ICommandValidator<CommandWithValidator> validator;
    private readonly IServiceProvider serviceProvider;
    private readonly CommandValidatorResolver resolver;

    public CommandValidatorResolverTests()
    {
        validator = Substitute.For<ICommandValidator<CommandWithValidator>>();
        serviceProvider = Substitute.For<IServiceProvider>();
        resolver = new CommandValidatorResolver(serviceProvider);

        serviceProvider.GetService(typeof(ICommandValidator<CommandWithValidator>)).Returns(validator);
    }

    [Fact]
    public void Returns_null_if_command_has_no_validator_registered()
    {
        var wrapper = resolver.FindCommandValidator(typeof(CommandWithoutValidator));
        Assert.Null(wrapper);
    }

    [Fact]
    public async Task Returns_wrapper_which_passes_command_to_validator_if_command_has_validator_registered()
    {
        var command = new CommandWithValidator();
        var wrapper = resolver.FindCommandValidator(typeof(CommandWithValidator));
        validator.ValidateAsync(Arg.Any<HttpContext>(), command).Returns(new ValidationResult(null));

        Assert.NotNull(wrapper);
        var result = await wrapper.ValidateAsync(Substitute.For<HttpContext>(), command);
        Assert.True(result.IsValid);
    }

    public class CommandWithValidator : ICommand { }

    public class CommandWithoutValidator : ICommand { }
}
