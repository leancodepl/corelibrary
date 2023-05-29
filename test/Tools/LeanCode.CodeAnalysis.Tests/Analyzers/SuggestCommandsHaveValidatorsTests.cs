using System.IO;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class SuggestCommandsHaveValidatorsTests : DiagnosticVerifier
{
    [Fact]
    public async Task Ignores_commands_with_validators_reports_not_validated()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Command_validation.cs");
        var diag = new DiagnosticResult(DiagnosticsIds.CommandsShouldHaveValidators, 18, 13);
        await VerifyDiagnostics(source, diag);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new SuggestCommandsHaveValidators();
    }
}
