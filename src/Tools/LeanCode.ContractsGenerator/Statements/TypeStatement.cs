using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    internal class TypeStatement : INamespacedStatement
    {
        public string Name { get; set; } = "";
        public string Namespace { get; set; } = "";
        public bool IsDictionary { get; set; }
        public bool IsNullable { get; set; }
        public bool IsArrayLike { get; set; }
        public List<TypeStatement> ParentChain { get; set; } = new List<TypeStatement>();
        public List<TypeStatement> TypeArguments { get; set; } = new List<TypeStatement>();
    }
}
