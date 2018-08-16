using System.Collections.Generic;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages
{
    public class LanguageFileOutput
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    interface ILanguageVisitor
    {
        IEnumerable<LanguageFileOutput> Visit(ClientStatement statement);
    }
}

