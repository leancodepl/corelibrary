using System.Collections.Generic;
using LeanCode.ContractsGenerator.Statements;

namespace LeanCode.ContractsGenerator.Languages
{
    public class LanguageFileOutput
    {
        public string Name { get; }
        public string Content { get; }

        public LanguageFileOutput(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    internal interface ILanguageVisitor
    {
        IEnumerable<LanguageFileOutput> Visit(ClientStatement statement);
    }
}
