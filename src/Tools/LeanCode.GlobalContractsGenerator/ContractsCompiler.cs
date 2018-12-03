using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LeanCode.CQRS;
using LeanCode.CQRS.Security;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LeanCode.ContractsGenerator
{
    class ContractsCompiler
    {
        private static readonly List<MetadataReference> DefaultAssemblies = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IQuery<,>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICustomAuthorizer<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HashSet<>).GetTypeInfo().Assembly.Location)
        };

        private readonly string rootPath;
        private readonly string contractsRegex;
        private readonly string additionalCode;

        public ContractsCompiler(GeneratorConfiguration configuration)
        {
            rootPath = configuration.RootPath;
            contractsRegex = configuration.ContractsRegex;
            additionalCode = configuration.AdditionalCode;
        }

        public CSharpCompilation GetCompilation(out List<SyntaxTree> trees)
        {
            trees = new List<SyntaxTree>();

            var fileRoot = new DirectoryInfo(rootPath);
            var fileRegex = new Regex(contractsRegex);

            var contracts = fileRoot.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => fileRegex.IsMatch(f.FullName));

            foreach (var contract in contracts)
            {
                using (var fileReader = new StreamReader(contract.OpenRead()))
                {
                    var contractText = fileReader.ReadToEnd();
                    var contractTree = CSharpSyntaxTree.ParseText(contractText);

                    trees.Add(contractTree);
                }
            }

            var compilation = CSharpCompilation.Create("LeanCode.ContractsGenerator")
                .AddReferences(DefaultAssemblies)
                .AddSyntaxTrees(trees.Concat(new[] { CSharpSyntaxTree.ParseText(additionalCode) }));

            return compilation;
        }
    }
}
