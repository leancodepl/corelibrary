namespace LeanCode.ContractsGenerator.Statements
{
    internal class FieldStatement : IStatement
    {
        public string Name { get; }
        public TypeStatement Type { get; }

        public FieldStatement(string name, TypeStatement type)
        {
            Name = name;
            Type = type;
        }
    }
}
