namespace LeanCode.ContractsGenerator.Statements
{
    internal class EnumValueStatement : IStatement
    {
        public string Name { get; set; } = "";
        public string? Value { get; set; } = "";
    }
}
