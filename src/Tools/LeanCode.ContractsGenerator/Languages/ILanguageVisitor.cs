using System.Collections.Generic;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages
{
    public class LanguageFileOutput
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    internal interface ILanguageVisitor
    {
        IEnumerable<LanguageFileOutput> Visit(ClientStatement statement);
    }
}
