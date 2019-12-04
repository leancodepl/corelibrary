using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.ContractsGenerator.Languages;

namespace LeanCode.ContractsGenerator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Contains("--help"))
            {
                Usage();
                return;
            }

            List<GeneratorConfiguration> configurations;
            string configFilePath;
            try
            {
                configFilePath = GeneratorConfiguration.GetConfigFile(args);
                configurations = await GeneratorConfiguration.GetConfigurations(configFilePath);
            }
            catch (FormatException e)
            {
                Usage(e.Message);
                return;
            }

            var configDir = Path.GetDirectoryName(configFilePath);
            if (configDir == null)
            {
                Usage("Configuration file does not exist or is invalid");
                return;
            }

            foreach (var config in configurations)
            {
                var compilation = new ContractsCompiler(config, configDir).GetCompilation(out var trees);
                var generator = new CodeGenerator(trees, compilation);

                SaveContracts(config, generator.Generate(config), configDir);
            }
        }

        private static void SaveContracts(GeneratorConfiguration config, IEnumerable<LanguageFileOutput> outputs, string configDir)
        {
            foreach (var output in outputs)
            {
                using (var fileWriter = new StreamWriter(new FileStream(Path.Combine(configDir, config.OutPath, output.Name), FileMode.Create)))
                {
                    fileWriter.Write(output.Content);
                }
            }
        }

        private static void Usage(string? errorMessage = null)
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
