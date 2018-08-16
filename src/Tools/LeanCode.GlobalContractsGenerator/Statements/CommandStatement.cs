namespace LeanCode.ContractsGenerator.Statements
{
    class CommandStatement : InterfaceStatement
    {
        public string NamespaceName { get; set; } = string.Empty;

        public CommandStatement() { }
        public CommandStatement(InterfaceStatement interfaceStatement)
        {
            Name = interfaceStatement.Name;
            IsStatic = interfaceStatement.IsStatic;
            Parameters = interfaceStatement.Parameters;
            Extends = interfaceStatement.Extends;
            Fields = interfaceStatement.Fields;
            Constants = interfaceStatement.Constants;
            Children = interfaceStatement.Children;
        }
    }
}

