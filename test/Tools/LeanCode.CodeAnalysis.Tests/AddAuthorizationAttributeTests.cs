using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests
{
    public class AddAuthorizationAttributeTests : CodeFixVerifier
    {
        [Fact]
        public async Task Adds_attribute()
        {
            var source =
@"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

public class Cmd : ICommand
{}";

            var expected =
            @"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

[AllowUnauthorized]
public class Cmd : ICommand
{}";

            await VerifyCSharpFix(source, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AddAuthorizationAttributeCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EnsureCommandsHaveAuthorizers();
        }
    }
}
