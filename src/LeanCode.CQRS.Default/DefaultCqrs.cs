using System.Threading.Tasks;

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

        public Task<CommandResult> ExecuteAsync<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            return commandExecutor.ExecuteAsync(command);
        }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            return queryExecutor.QueryAsync(query);
        }
    }
}
