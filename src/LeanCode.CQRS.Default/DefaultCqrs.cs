namespace LeanCode.CQRS.Default
{
    public class DefaultCqrs : ICqrs
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly IQueryExecutor queryExecutor;

        public DefaultCqrs(
            ICommandExecutor commandExecutor,
            IQueryExecutor queryExecutor)
        {
            this.commandExecutor = commandExecutor;
            this.queryExecutor = queryExecutor;
        }

        public CommandResult Execute<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            return commandExecutor.Execute(command);
        }

        public TResult Execute<TResult>(IQuery<TResult> query)
        {
            return queryExecutor.Execute(query);
        }
    }
}
