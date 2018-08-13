using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LeanCode.ContractsGenerator
{
    public class GeneratorConfiguration
    {
        public string RootPath { get; set; } = Directory.GetCurrentDirectory();
        public string ContractsRegex { get; set; } = @".*\.cs$";
        public string OutPath { get; set; } = Directory.GetCurrentDirectory();
        public string Name { get; set; } = "contracts";
        public string AdditionalCode { get; set; } = "";
        public string ContractsPreamble { get; set; } = "import { IRemoteQuery, IRemoteCommand } from \"@leancode/cqrs-client/CQRS\";\n\n";
        public string ClientPreamble { get; set; } = "import { CommandResult, CQRS } from \"@leancode/cqrs-client/CQRS\";\nimport { ClientType } from \"@leancode/cqrs-client/ClientType\";\n";
        public Dictionary<string, string> TypeTranslations = new Dictionary<string, string>
        {
            { "int", "number" },
            { "double", "number" },
            { "float", "number" },
            { "single", "number" },
            { "int32", "number" },
            { "uint32", "number" },
            { "byte", "number" },
            { "sbyte", "number" },
            { "int64", "number" },
            { "short", "number" },
            { "long", "number" },
            { "decimal", "number" },
            { "bool", "boolean" },
            { "boolean", "boolean" },
            { "datetime", "string" },
            { "timespan", "string" },
            { "datetimeoffset", "string"},
            { "guid", "string" },
            { "string", "string" },
            { "jobject", "any" },
            { "dynamic", "any" },
            { "object", "any" }
        };


        public GeneratorConfiguration()
        { }

        public static List<GeneratorConfiguration> GetConfigurations(string[] args)
        {
            var configFile = GetConfigFile(args);

            string content;
            using (var reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), configFile)))
            {
                content = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<List<GeneratorConfiguration>>(content);
        }

        private static string GetConfigFile(string[] args)
        {
            const string configFileParameterName = "configFile";
            const string defaultConfigFile = "contracts-config.json";

            var commandLineMappings = new Dictionary<string, string>
            {
                { "-c", configFileParameterName }
            };

            return new ConfigurationBuilder()
                .AddCommandLine(args, commandLineMappings).Build()
                .GetValue(configFileParameterName, defaultConfigFile);
        }
    }
}
