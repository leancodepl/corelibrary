using FluentValidation;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace ValidatedCommands;

public class ValidatedCommand : ICommand { }

public class Validator : AbstractValidator<ValidatedCommand> { }

public class ValidatedHandler : ICommandHandler<ValidatedCommand>
{
    public Task ExecuteAsync(HttpContext context, ValidatedCommand command) => Task.CompletedTask;
}

public class NotValidatedCommand : ICommand { }

public class NotValidatedHandler : ICommandHandler<NotValidatedCommand>
{
    public Task ExecuteAsync(HttpContext context, NotValidatedCommand command) => Task.CompletedTask;
}
