using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.AspNetCore.Http;

namespace ValidatedCommands;

public class ValidatedCommand : ICommand { }

public class Validator : ContextualValidator<ValidatedCommand> { }

public class ValidatedHandler : ICommandHandler<ValidatedCommand>
{
    public Task ExecuteAsync(HttpContext context, ValidatedCommand command) => Task.CompletedTask;
}

public class NotValidatedCommand : ICommand { }

public class NotValidatedHandler : ICommandHandler<NotValidatedCommand>
{
    public Task ExecuteAsync(HttpContext context, NotValidatedCommand command) => Task.CompletedTask;
}
