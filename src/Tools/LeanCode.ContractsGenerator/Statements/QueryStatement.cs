namespace LeanCode.ContractsGenerator.Statements
{
    internal class QueryStatement : InterfaceStatement
    {
        public QueryStatement() { }
        public TypeStatement ResultType { get; set; }

        public QueryStatement(InterfaceStatement interfaceStatement)
        {
            Name = interfaceStatement.Name;
            Namespace = interfaceStatement.Namespace;
            IsClass = interfaceStatement.IsClass;
            BaseClass = interfaceStatement.BaseClass;
            IsStatic = interfaceStatement.IsStatic;
            Parameters = interfaceStatement.Parameters;
            Extends = interfaceStatement.Extends;
            Fields = interfaceStatement.Fields;
            Constants = interfaceStatement.Constants;
            Children = interfaceStatement.Children;
        }
    }
}
