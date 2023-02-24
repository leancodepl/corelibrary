using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

namespace ValidatedCommands;

public class Context { }

public class ValidatedCommand : ICommand { }

public class Validator : ContextualValidator<ValidatedCommand> { }

public class ValidatedHandler : ICommandHandler<Context, ValidatedCommand>
{
    public Task ExecuteAsync(Context context, ValidatedCommand command) => Task.CompletedTask;
}

public class NotValidatedCommand : ICommand { }

public class NotValidatedHandler : ICommandHandler<Context, NotValidatedCommand>
{
    public Task ExecuteAsync(Context context, NotValidatedCommand command) => Task.CompletedTask;
}
