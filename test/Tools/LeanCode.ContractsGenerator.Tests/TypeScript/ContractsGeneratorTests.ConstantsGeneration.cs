using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public class ContractsGeneratorTests_ConstantsGeneration
    {
        [Fact]
        public void Commands_error_code_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; } }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Invalid: 1", client);
        }

        [Fact]
        public void Const_string_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class Constants { public const string Constant = nameof(Constant); }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Constant: \"Constant\"", client);
        }

        [Fact]
        public void Const_in_nested_static_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class Constants { public static class Constants2 { public const char Value = 'p'; } }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Constants = {", client);
            Assert.Contains("Constants2: {", client);
            Assert.Contains("Value: \"p\"", client);
        }

        [Fact]
        public void Multiple_command_error_codes_are_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; public const int Empty = 2; } }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Invalid: 1", client);
            Assert.Contains("Empty: 2", client);
        }

        [Fact]
        public void Const_double_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class Constants { public const double Constant = 1.55; }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Constant: 1.55", client);
        }

        [Fact]
        public void Const_float_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class Constants { public const float Constant = 1.55E+1; }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Constant: 15.5", client);
        }

        [Fact]
        public void Const_char_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class Constants { public const char Constant = 'a'; }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Constant: \"a\"", client);
        }
    }
}
