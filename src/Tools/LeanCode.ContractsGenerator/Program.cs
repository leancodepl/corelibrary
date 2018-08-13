using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeanCode.ContractsGenerator
{
    class Program
    {
        static void Main(string[] args)
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
                new CodeGenerator(trees, compilation, config).Generate(out var contracts, out var client);

                SaveContracts(config, contracts, client);
            }
        }

        private static void SaveContracts(GeneratorConfiguration config, string contracts, string client)
        {
            using (var fileWriter = new StreamWriter(new FileStream(Path.Combine(config.OutPath, config.Name + "Client.ts"), FileMode.Create)))
            {
                fileWriter.Write(client);
            }

            using (var fileWriter = new StreamWriter(new FileStream(Path.Combine(config.OutPath, config.Name + ".d.ts"), FileMode.Create)))
            {
                fileWriter.Write(contracts);
            }
        }

        static void Usage(string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                Console.WriteLine(errorMessage);
                Console.WriteLine();
            }

            Console.WriteLine("LeanCode .ts contracts generator");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet generate [options]");
            Console.WriteLine();
            Console.WriteLine("options:");
            Console.WriteLine("  -c, --configFile      path to configuration file");
            Console.WriteLine("  --help                prints this message");
        }
    }
}
