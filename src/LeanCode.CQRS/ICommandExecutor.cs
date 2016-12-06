namespace LeanCode.CQRS
{
    public interface ICommandExecutor
    {
        CommandResult Execute<TCommand>(TCommand command)
            where TCommand : ICommand;
    }
}
