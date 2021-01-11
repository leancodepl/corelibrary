using Xunit;
using static LeanCode.ContractsGenerator.Tests.Dart.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.Dart
{
    public class ContractsGeneratorTests_ConstantsGeneration
    {
        [Fact]
        public void Commands_error_code_is_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("static const invalid = 1", contracts);
        }

        [Fact]
        public void Const_in_nested_static_class_is_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class Constants { public static class Constants2 { public const int Value = 1; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("Constants {", contracts);
            Assert.Contains("Constants2 {", contracts);
            Assert.Contains("static const value = 1", contracts);
        }

        [Fact]
        public void Multiple_command_error_codes_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public class TestClass : IRemoteCommand { public static class ErrorCodes { public const int Invalid = 1; public const int Empty = 2; } }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("static const invalid = 1", contracts);
            Assert.Contains("static const empty = 2", contracts);
        }

        [Fact]
        public void Constants_in_static_classes_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class StaticClass { public const int SomeConstant = 1; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("StaticClass {", contracts);
            Assert.Contains("static const someConstant = 1", contracts);
        }

        [Fact]
        public void String_constants_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class StaticClass { public const string SomeConstant = \"TEST\"; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("StaticClass {", contracts);
            Assert.Contains("static const someConstant = \"TEST\"", contracts);
        }

        [Fact]
        public void Double_constants_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class StaticClass { public const double SomeConstant = 1.23; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("StaticClass {", contracts);
            Assert.Contains("static const someConstant = 1.23", contracts);
        }

        [Fact]
        public void Float_constants_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class StaticClass { public const float SomeConstant = 1.23; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("StaticClass {", contracts);
            Assert.Contains("static const someConstant = 1.23", contracts);
        }

        [Fact]
        public void Char_constants_are_generated()
        {
            var generator = CreateDartGeneratorFromNamespace("public static class StaticClass { public const char SomeConstant = 'a'; }");

            var contracts = GetContracts(generator.Generate(DefaultDartConfiguration));

            Assert.Contains("StaticClass {", contracts);
            Assert.Contains("static const someConstant = \"a\"", contracts);
        }
    }
}
