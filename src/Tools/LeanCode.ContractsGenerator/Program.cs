using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeanCode.ContractsGenerator.Languages;

namespace LeanCode.ContractsGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Contains("--help"))
            {
                Usage();
                return;
            }

            List<GeneratorConfiguration> configurations;
            try
            {
                configurations = GeneratorConfiguration.GetConfigurations(args);
            }
            catch (FormatException e)
            {
                Usage(e.Message);
                return;
            }

            foreach (var config in configurations)
            {
                var compilation = new ContractsCompiler(config).GetCompilation(out var trees);
                var generator = new CodeGenerator(trees, compilation);

                SaveContracts(config, generator.Generate(config));
            }
        }

        private static void SaveContracts(GeneratorConfiguration config, IEnumerable<LanguageFileOutput> outputs)
        {
            foreach (var output in outputs)
            {
                using (var fileWriter = new StreamWriter(new FileStream(Path.Combine(config.OutPath, output.Name), FileMode.Create)))
                {
                    fileWriter.Write(output.Content);
                }
            }
        }

        private static void Usage(string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                Console.WriteLine(errorMessage);
                Console.WriteLine();
            }

            Console.WriteLine("LeanCode .ts/.dart contracts generator");
            Console.WriteLine($"Version: {typeof(Program).Assembly.GetName().Version}");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet generate [options]");
            Console.WriteLine();
            Console.WriteLine("options:");
            Console.WriteLine("  -c, --configFile      path to configuration file");
            Console.WriteLine("  --help                prints this message");
        }
    }
}
