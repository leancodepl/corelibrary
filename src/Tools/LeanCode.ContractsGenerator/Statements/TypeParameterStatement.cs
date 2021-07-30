using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    internal class TypeParameterStatement : IStatement
    {
        public string Name { get; set; } = "";
        public List<TypeStatement> Constraints { get; set; } = new List<TypeStatement>();
    }
}
