using System.Collections.Generic;
using System.IO;
using LeanCode.ContractsGenerator.Languages.Dart;
using LeanCode.ContractsGenerator.Languages.TypeScript;
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
        public string AdditionalCode { get; set; } = string.Empty;

        public TypeScriptConfiguration TypeScript { get; set; }
        public DartConfiguration Dart { get; set; }

        public GeneratorConfiguration()
        { }

        public static List<GeneratorConfiguration> GetConfigurations(string[] args)
        {
            var configFile = GetConfigFile(args);

            using (var reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), configFile)))
            {
                return DeserializeConfigurationsFromString(reader.ReadToEnd());
            }
        }

        public static List<GeneratorConfiguration> DeserializeConfigurationsFromString(string configurations)
        {
            return JsonConvert.DeserializeObject<List<GeneratorConfiguration>>(configurations);
        }

        private static string GetConfigFile(string[] args)
        {
            const string configFileParameterName = "configFile";
            const string defaultConfigFile = "contracts-config.json";

            var commandLineMappings = new Dictionary<string, string>
            {
                { "-c", configFileParameterName },
            };

            return new ConfigurationBuilder()
                .AddCommandLine(args, commandLineMappings).Build()
                .GetValue(configFileParameterName, defaultConfigFile);
        }
    }
}
