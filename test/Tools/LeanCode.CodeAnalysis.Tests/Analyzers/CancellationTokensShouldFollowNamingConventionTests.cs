using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class CancellationTokensShouldFollowNamingConventionTests : DiagnosticVerifier
{
    [Fact]
    public async Task Methods_following_CancellationToken_naming_convention_are_accepted()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Accepted_methods.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Methods_not_following_CancellationToken_naming_convention_are_rejected()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Rejected_methods.cs");

        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.CancellationTokensShouldFollowNamingConvention, 4, 89),
        };

        await VerifyDiagnostics(source, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new CancellationTokensShouldFollowNamingConvention();
    }
}
