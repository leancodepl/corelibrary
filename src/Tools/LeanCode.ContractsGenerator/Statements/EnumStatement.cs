using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    internal class EnumStatement : INamespacedStatement
    {
        public string Name { get; set; } = "";
        public string Namespace { get; set; } = "";
        public List<EnumValueStatement> Values { get; set; } = new List<EnumValueStatement>();
    }
}
