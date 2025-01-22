using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LeanCode.TestBed.Api;

[AllowUnauthorized]
public class TestCommand : ICommand
{
    public required bool TriggerCode1 { get; set; }
    public required bool TriggerCode2 { get; set; }
    public required string OtherParameter { get; set; }

    public static class ErrorCodes
    {
        public const int Code1 = 1;
        public const int Code2 = 2;
    }
}

public class TestCommandCV : AbstractValidator<TestCommand>
{
    public TestCommandCV()
    {
        RuleFor(e => e.TriggerCode1).Equal(false).WithCode(TestCommand.ErrorCodes.Code1);
        RuleFor(e => e.TriggerCode2).Equal(false).WithCode(TestCommand.ErrorCodes.Code2);
    }
}

public class TestCommandCH : ICommandHandler<TestCommand>
{
    private readonly ILogger logger = Log.ForContext<TestCommandCH>();

    public Task ExecuteAsync(HttpContext context, TestCommand command)
    {
        logger.Information("Executing command {Command}", command);
        return Task.CompletedTask;
    }
}
