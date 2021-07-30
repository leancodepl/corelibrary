namespace LeanCode.ContractsGenerator.Statements
{
    internal class ConstStatement : IStatement
    {
        public string Name { get; set; } = "";
        public string? Value { get; set; }
    }
}
