namespace LeanCode.ContractsGenerator.Statements
{
    class QueryStatement : InterfaceStatement
    {
        public string NamespaceName { get; set; } = string.Empty;

        public QueryStatement() { }
        public QueryStatement(InterfaceStatement interfaceStatement)
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

