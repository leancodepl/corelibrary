using Xunit;
using static LeanCode.ContractsGenerator.Tests.TypeScript.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests.TypeScript
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Private_class_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.DoesNotContain("interface TestClass", contracts);
        }

        [Fact]
        public void Protected_class_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("private class TestClass { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.DoesNotContain("interface TestClass", contracts);
        }

        [Fact]
        public void Class_with_no_access_modifier_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("class TestClass { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.DoesNotContain("interface TestClass", contracts);
        }

        [Fact]
        public void Public_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass", contracts);
        }

        [Fact]
        public void Generic_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass<T> { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass<T>", contracts);
        }

        [Fact]
        public void Generic_class_with_constraints_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt { } public class TestClass<T> where T: IList<IInt> { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass<T extends IInt[]>", contracts);
        }

        [Fact]
        public void Class_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt { } public class TestClass : IInt {}");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass extends IInt", contracts);
        }

        [Fact]
        public void Generic_class_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<T> {}");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass<T> extends IInt<T>", contracts);
        }

        [Fact]
        public void Deep_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass : IInt<List<int>> {}");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass extends IInt<number[]>", contracts);
        }

        [Fact]
        public void Deep_generic_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<List<T>> { }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass<T> extends IInt<T[]>", contracts);
        }

        [Fact]
        public void Nested_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : List<Result> { public class Result {} }");

            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass extends Result[] {", contracts);
            Assert.Contains("interface TestClass_Result {", contracts);
        }

        [Fact]
        public void Static_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class ErrorCodes { public const int Invalid = 1; }");

            var client = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("Invalid: 1", client);
            Assert.Contains("ErrorCodes = {", client);
        }

        [Fact]
        public void Order_of_classes_is_alphanumeric()
        {
            var file1 = @"
            using LeanCode.CQRS;
            namespace File2
            {
                public class TestClass4 { }
                public class TestClass3 { }
            }";

            var file2 = @"
            using LeanCode.CQRS;
            namespace File1
            {
                public class TestClass2 { }
                public class TestClass1 { }
            }";

            var generator = CreateTsGenerator(file1, file2);
            var contracts = GetClient(generator.Generate(DefaultTypeScriptConfiguration));

            Assert.Contains("interface TestClass1", contracts);
            Assert.Contains("interface TestClass2", contracts);
            Assert.Contains("interface TestClass3", contracts);
            Assert.Contains("interface TestClass4", contracts);

            var test1Index = contracts.IndexOf("interface TestClass1");
            var test2Index = contracts.IndexOf("interface TestClass2");
            var test3Index = contracts.IndexOf("interface TestClass3");
            var test4Index = contracts.IndexOf("interface TestClass4");

            Assert.True(test1Index < test2Index);
            Assert.True(test2Index < test3Index);
            Assert.True(test3Index < test4Index);
        }
    }
}
