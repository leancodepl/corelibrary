using System.Linq;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions
{
    public class AddAuthorizationAttributeCodeActionTests : CodeFixVerifier
    {
        private static readonly string[] StaticFixes = new[]
        {
            "Add AllowUnauthorizedAttribute",
            "Add AuthorizeWhenHasAnyOfAttribute",
        };

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

            await VerifyCodeFix(source, expected, StaticFixes, 0);
        }

        [Fact]
        public async Task Adds_attribute_and_using_directive()
        {
            var source =
@"
using LeanCode.CQRS;

public class Cmd : ICommand
{}";

            var expected =
            @"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

[AllowUnauthorized]
public class Cmd : ICommand
{}";

            await VerifyCodeFix(source, expected, StaticFixes, 0);
        }

        [Fact]
        public async Task Suggests_custom_attribute()
        {
            var source =
            @"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

public class CustomAuthorizeAttribute : AuthorizeWhenAttribute
{
    public CustomAuthorizeAttribute() : base(typeof(object))
    {}
}

public class Cmd : ICommand
{}";

            var expected =
            @"
using LeanCode.CQRS;
using LeanCode.CQRS.Security;

public class CustomAuthorizeAttribute : AuthorizeWhenAttribute
{
    public CustomAuthorizeAttribute() : base(typeof(object))
    {}
}

[CustomAuthorize]
public class Cmd : ICommand
{}";

            var fixes = new[]
            {
                "Add AllowUnauthorizedAttribute",
                "Add AuthorizeWhenHasAnyOfAttribute",
                "Add CustomAuthorizeAttribute",
            };

            await VerifyCodeFix(source, expected, fixes, 2);
        }

        protected override CodeFixProvider GetCodeFixProvider()
        {
            return new AddAuthorizationAttributeCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new EnsureCommandsAndQueriesHaveAuthorizers();
        }
    }
}
