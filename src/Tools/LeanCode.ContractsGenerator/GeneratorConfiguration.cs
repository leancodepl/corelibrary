using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.ContractsGenerator.Languages.Dart;
using LeanCode.ContractsGenerator.Languages.TypeScript;
using Microsoft.Extensions.Configuration;

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

        public GeneratorConfiguration() { }

        public static async Task<List<GeneratorConfiguration>> GetConfigurations(string configFile)
        {
            await using var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), configFile));
            return await JsonSerializer.DeserializeAsync<List<GeneratorConfiguration>>(stream);
        }

        public static List<GeneratorConfiguration> DeserializeConfigurationsFromString(string cfg)
        {
            return JsonSerializer.Deserialize<List<GeneratorConfiguration>>(cfg);
        }

        public static string GetConfigFile(string[] args)
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
