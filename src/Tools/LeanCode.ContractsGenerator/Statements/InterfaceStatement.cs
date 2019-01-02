using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    class InterfaceStatement : INamespacedStatement
    {
        public string Name { get; set; } = string.Empty;
        public bool IsStatic { get; set; } = false;
        public bool IsClass { get; set; } = false;
        public string Namespace { get; set; } = string.Empty;
        public List<TypeParameterStatement> Parameters { get; set; } = new List<TypeParameterStatement>();
        public List<TypeStatement> Extends { get; set; } = new List<TypeStatement>();
        public List<FieldStatement> Fields { get; set; } = new List<FieldStatement>();
        public List<ConstStatement> Constants { get; set; } = new List<ConstStatement>();
        public List<InterfaceStatement> Children { get; set; } = new List<InterfaceStatement>();
    }
}

