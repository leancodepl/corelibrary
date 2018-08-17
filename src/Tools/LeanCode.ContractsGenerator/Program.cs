using System;
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

            GeneratorConfiguration config;
            try
            {
                config = GeneratorConfiguration.CreateFromArgs(args);
            }
            catch (FormatException e)
            {
                Usage(e.Message);
                return;
            }

            var compilation = new ContractsCompiler(config).GetCompilation(out var trees);
            new CodeGenerator(trees, compilation, config).Generate(out var contracts, out var client);

            SaveContracts(config, contracts, client);
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
            Console.WriteLine("  -r, --RootPath        root directory in which to seek for .cs contracts file");
            Console.WriteLine("  -c, --ContractsRegex  regex expression matching .cs files for output");
            Console.WriteLine("  -o, --OutPath         directory where to output files");
            Console.WriteLine("  -n, --Name            name of files to output, output consist of two files [name].ts and [name].d.ts");
            Console.WriteLine("  --AdditionalCode      default code to be included during compilation");
            Console.WriteLine("  --ClientPreamble      .ts client preamble");
            Console.WriteLine("  --ContractsPreamble   .d.ts contracts preamble");
            Console.WriteLine("  --help                prints this message");
        }
    }
}
