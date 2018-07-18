using Xunit;
using static LeanCode.ContractsGenerator.Tests.ContractsGeneratorTestHelpers;

namespace LeanCode.ContractsGenerator.Tests
{
    public partial class ContractsGeneratorTests
    {
        [Fact]
        public void Private_class_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("private class TestClass { }");

            generator.Generate(out var contracts, out var client);

            Assert.DoesNotMatch("interface TestClass", contracts);
        }

        [Fact]
        public void Protected_class_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("private class TestClass { }");

            generator.Generate(out var contracts, out var client);

            Assert.DoesNotMatch("interface TestClass", contracts);
        }

        [Fact]
        public void Class_with_no_access_modifier_is_not_resolved()
        {
            var generator = CreateTsGeneratorFromNamespace("class TestClass { }");

            generator.Generate(out var contracts, out var client);

            Assert.DoesNotMatch("interface TestClass", contracts);
        }

        [Fact]
        public void Public_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass { }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass", contracts);
        }

        [Fact]
        public void Generic_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass<T> { }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass<T>", contracts);
        }

        [Fact]
        public void Generic_class_with_constraints_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt { } public class TestClass<T> where T: IList<IInt> { }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass<T extends IInt\\[\\]>", contracts);
        }

        [Fact]
        public void Class_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt { } public class TestClass : IInt {}");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass extends IInt", contracts);
        }

        [Fact]
        public void Generic_class_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<T> {}");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass<T> extends IInt<T>", contracts);
        }

        [Fact]
        public void Deep_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass : IInt<List<int>> {}");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass extends IInt<number\\[\\]>", contracts);
        }

        [Fact]
        public void Deep_generic_inheritance_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public interface IInt<T> { } public class TestClass<T> : IInt<List<T>> { }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass<T> extends IInt<T\\[\\]>", contracts);
        }

        [Fact]
        public void Nested_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public class TestClass : List<Result> { public class Result {} }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("interface TestClass extends Result\\[\\] \\{", contracts);
            Assert.Matches("interface Result \\{", contracts);
        }

        [Fact]
        public void Static_class_class_is_resolved_correctly()
        {
            var generator = CreateTsGeneratorFromNamespace("public static class ErrorCodes { public const int Invalid = 1; }");

            generator.Generate(out var contracts, out var client);

            Assert.Matches("export const Invalid = 1;", client);
            Assert.Matches("export namespace ErrorCodes {", client);
        }
    }
}
