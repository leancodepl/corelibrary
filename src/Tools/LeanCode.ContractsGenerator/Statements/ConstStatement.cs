namespace LeanCode.ContractsGenerator.Statements
{
    class ConstStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

