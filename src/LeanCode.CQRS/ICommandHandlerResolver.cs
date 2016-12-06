namespace LeanCode.CQRS
{
    public interface ICommandHandlerResolver
    {
        ICommandHandler<TCommand> FindCommandHandler<TCommand>()
            where TCommand : ICommand;
    }
}
