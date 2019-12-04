namespace LeanCode.ContractsGenerator.Statements
{
    internal class QueryStatement : InterfaceStatement
    {
        public TypeStatement ResultType { get; set; }

        public QueryStatement(
            InterfaceStatement interfaceStatement,
            string ns,
            TypeStatement resultType)
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
            Namespace = ns;
            ResultType = resultType;
        }
    }
}
