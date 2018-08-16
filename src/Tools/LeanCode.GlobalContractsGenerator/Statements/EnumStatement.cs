using System.Collections.Generic;

namespace LeanCode.ContractsGenerator.Statements
{
    class EnumStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public List<EnumValueStatement> Values { get; set; } = new List<EnumValueStatement>();
    }
}

