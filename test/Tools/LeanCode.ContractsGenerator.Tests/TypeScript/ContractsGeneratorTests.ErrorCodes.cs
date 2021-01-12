using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public class ContractsGeneratorTests_ErrorCodes
    {
        [Fact]
        public void ErrorCodes_are_resolved_correctly_with_default_config()
        {
            var generator = CreateTsGeneratorFromNamespace(@"
                public class TestClass : IRemoteCommand
                {
                    public static class ErrorCodes
                    {
                        public const int InvalidX = 0;
                    }
                }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("typeof TestClass[\"ErrorCodes\"]", contracts);
        }

        [Fact]
        public void ErrorCodes_are_resolved_correctly_with_overridden_config()
        {
            var configuration = GeneratorConfiguration.DeserializeConfigurationsFromString(@"
                [
                    {
                        ""ErrorCodesName"": ""ValidationErrors"",
                        ""TypeScript"": { }
                    }
                ]")[0];
            var generator = CreateTsGeneratorFromNamespace(@"
                public class TestClass : IRemoteCommand
                {
                    public static class ValidationErrors
                    {
                        public const int InvalidX = 0;
                    }
                }");

            var contracts = GetClient(generator.Generate(configuration));

            Assert.Contains("typeof TestClass[\"ValidationErrors\"]", contracts);
        }
    }
}
