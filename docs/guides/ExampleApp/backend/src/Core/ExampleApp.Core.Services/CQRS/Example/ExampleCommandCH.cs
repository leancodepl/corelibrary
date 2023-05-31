using ExampleApp.Core.Contracts.Example;
using FluentValidation;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

namespace ExampleApp.Core.Services.CQRS.Example;

public class ExampleCommandCV : ContextualValidator<ExampleCommand>
{
    public ExampleCommandCV()
    {
        RuleFor(cmd => cmd.Arg).NotEmpty().WithCode(ExampleCommand.ErrorCodes.EmptyArg);
    }
}

public class ExampleCommandCH : ICommandHandler<CoreContext, ExampleCommand>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ExampleCommandCH>();

    public Task ExecuteAsync(CoreContext context, ExampleCommand command)
    {
        logger.Information("ExampleCommandCH called");
        return Task.CompletedTask;
    }
}
