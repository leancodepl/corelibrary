namespace LeanCode.CQRS
{
    public interface ICqrs : IQueryExecutor, ICommandExecutor
    { }
}
