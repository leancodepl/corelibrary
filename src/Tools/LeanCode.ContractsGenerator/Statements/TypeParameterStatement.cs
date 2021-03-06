using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    internal class TypeParameterStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public List<TypeStatement> Constraints { get; set; } = new List<TypeStatement>();
    }
}
