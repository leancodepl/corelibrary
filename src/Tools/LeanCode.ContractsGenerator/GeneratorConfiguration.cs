using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LeanCode.ContractsGenerator
{
    public class GeneratorConfiguration
    {
        private const string DefaultAdditionalCode = "";

        private const string DefaultContractsPreamble = "import { IRemoteQuery, IRemoteCommand } from \"@leancode/cqrs-client/CQRS\";\n\n";
        private const string DefaultClientPreamble = "import { CommandResult, CQRS } from \"@leancode/cqrs-client/CQRS\";\nimport { ClientType } from \"@leancode/cqrs-client/ClientType\";\n";

        public string RootPath { get; set; }
        public string ContractsRegex { get; set; }
        public string OutPath { get; set; }
        public string Name { get; set; }
        public string AdditionalCode { get; set; }
        public string ContractsPreamble { get; set; }
        public string ClientPreamble { get; set; }

        private static readonly Dictionary<string, string> DefaultConfiguration = new Dictionary<string, string>
        {
            { nameof(RootPath), Directory.GetCurrentDirectory() },
            { nameof(ContractsRegex), @".*\.cs$" },
            { nameof(OutPath), Directory.GetCurrentDirectory() },
            { nameof(Name), "contracts" },
            { nameof(AdditionalCode), DefaultAdditionalCode },
            { nameof(ContractsPreamble), DefaultContractsPreamble },
            { nameof(ClientPreamble), DefaultClientPreamble },
        };

        private static readonly Dictionary<string, string> CommandLineSwitchMappings = new Dictionary<string, string>
        {
            { "-r", nameof(RootPath) },
            { "-c", nameof(ContractsRegex) },
            { "-o", nameof(OutPath) },
            { "-n", nameof(Name) }
        };

        public GeneratorConfiguration()
        { }

        public static GeneratorConfiguration CreateFromArgs(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(DefaultConfiguration)
                .AddCommandLine(args, CommandLineSwitchMappings);

            var configuration = builder.Build();

            return configuration.Get<GeneratorConfiguration>();
        }
    }
}
