using System.IO;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers
{
    public class EnsureAllValidationErrorCodesAreUsedTests : DiagnosticVerifier
    {
        [Fact]
        public async Task Does_not_report_if_all_codes_are_used()
        {
            var source = await File.ReadAllTextAsync("TestSamples/EnsureAllValidationErrorCodesAreUsedTests_Accepted_validators.cs");
            await VerifyDiagnostics(source);
        }

        [Fact]
        public async Task Reports_missing_validation_errors()
        {
            var source = await File.ReadAllTextAsync("TestSamples/EnsureAllValidationErrorCodesAreUsedTests_Rejected_validators.cs");
            var diag = new DiagnosticResult(DiagnosticsIds.AllValidationErrorCodesShouldBeUsed, 23, 17);
            await VerifyDiagnostics(source, diag);
        }

        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new EnsureAllValidationErrorCodesAreUsed();
        }
    }
}
