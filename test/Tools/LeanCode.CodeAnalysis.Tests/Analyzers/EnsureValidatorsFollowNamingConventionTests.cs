using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnsureValidatorsFollowNamingConventionTests : DiagnosticVerifier
{
    [Fact]
    public async Task Validators_following_naming_convention_are_accepted()
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Accepted/CQRS/Validators.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Validators_not_following_naming_convention_are_rejected()
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Rejected/CQRS/Validators.cs");

        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.CommandValidatorsShouldFollowNamingConvention, 9, 13),
        };

        await VerifyDiagnostics(source, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCommandValidatorsFollowNamingConvention();
    }
}
