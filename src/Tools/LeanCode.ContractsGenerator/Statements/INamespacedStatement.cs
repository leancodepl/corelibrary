namespace LeanCode.ContractsGenerator.Statements
{
    internal interface INamespacedStatement : IStatement
    {
        string Namespace { get; }
    }
}
