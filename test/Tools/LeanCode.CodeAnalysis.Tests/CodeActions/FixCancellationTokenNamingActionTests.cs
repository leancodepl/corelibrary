using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class FixCancellationTokenNamingActionTests : CodeFixVerifier
{
    [Fact]
    public async Task Renames_CancellationToken_argument_not_following_convention()
    {
        var source =
            @"
namespace Test;

public static class Test
{
    public static CancellationToken Method(CancellationToken ct)
    {
        var t = ct;
        return ct;
    }
}";

        var expected =
            @"
namespace Test;

public static class Test
{
    public static CancellationToken Method(CancellationToken cancellationToken)
    {
        var t = cancellationToken;
        return cancellationToken;
    }
}";

        var fixes = new[] { "Fix CancellationToken argument name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Theory]
    [InlineData("override")]
    [InlineData("new")]
    public async Task Does_not_rename_overriden_and_new_methods(string keyword)
    {
        var source =
            $@"
namespace Test;

public class Test : Base
{{
    public {keyword} CancellationToken NewMethod(CancellationToken ct) : BaseMethod
    {{
        return ct;
    }}

    public static CancellationToken Method(CancellationToken ct)
    {{
        return ct;
    }}
}}";

        var expected =
            $@"
namespace Test;

public class Test : Base
{{
    public {keyword} CancellationToken NewMethod(CancellationToken ct) : BaseMethod
    {{
        return ct;
    }}

    public static CancellationToken Method(CancellationToken cancellationToken)
    {{
        return cancellationToken;
    }}
}}";

        var fixes = new[] { "Fix CancellationToken argument name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new FixCancellationTokenNamingCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new CancellationTokensShouldFollowNamingConvention();
    }
}
