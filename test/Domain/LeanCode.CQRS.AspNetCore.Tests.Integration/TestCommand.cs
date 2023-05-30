using FluentValidation;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Tests.Integration;

[CustomAuthorizeWhen]
public class TestCommand : ICommand, ICustomAuthorizerParams
{
    public bool FailValidation { get; set; }
    public bool FailAuthorization { get; set; }

    public static class ErrorCodes
    {
        public const int ValidationError = 1;
    }
}

public class TestCommandValidator : ContextualValidator<TestCommand>
{
    public TestCommandValidator()
    {
        RuleFor(cmd => cmd.FailValidation)
            .Equal(false)
            .WithCode(TestCommand.ErrorCodes.ValidationError)
            .WithMessage("Test command should pass validation");
    }
}

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task ExecuteAsync(HttpContext context, TestCommand command)
    {
        return Task.CompletedTask;
    }
}

public class TestFailingCommand : ICommand { }

public class FailingCommandHandler : ICommandHandler<TestFailingCommand>
{
    public Task ExecuteAsync(HttpContext context, TestFailingCommand command)
    {
        throw new InvalidOperationException("This handler fails");
    }
}
