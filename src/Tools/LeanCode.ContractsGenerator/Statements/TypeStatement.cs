using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    class TypeStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDictionary { get; set; } = false;
        public bool IsNullable { get; set; } = false;
        public bool IsArrayLike { get; set; } = false;
        public List<TypeStatement> TypeArguments { get; set; } = new List<TypeStatement>();
    }
}

