namespace LeanCode.ContractsGenerator.Statements
{
    internal class FieldStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public TypeStatement Type { get; set; } = null;
    }
}
