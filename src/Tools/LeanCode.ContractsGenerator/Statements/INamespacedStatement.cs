namespace LeanCode.ContractsGenerator.Statements
{
    interface INamespacedStatement : IStatement
    {
        string Namespace { get; }
    }
}

