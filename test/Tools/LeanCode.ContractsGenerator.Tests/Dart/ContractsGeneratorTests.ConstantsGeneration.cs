using System.Text.RegularExpressions;
using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Commands_error_code_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Invalid = 1", contracts);
        }

        [Fact]
        public void Const_in_nested_static_class_is_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class Constants { public static class Constants2 { public const int Value = 1; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Constants {", contracts);
            Assert.Contains("Constants2 {", contracts);
            Assert.Contains("Value = 1", contracts);
        }

        [Fact]
        public void Multiple_command_error_codes_are_resolved_correctly()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; public const int Empty = 2; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Invalid = 1", contracts);
            Assert.Contains("Empty = 2", contracts);
        }
    }
}
