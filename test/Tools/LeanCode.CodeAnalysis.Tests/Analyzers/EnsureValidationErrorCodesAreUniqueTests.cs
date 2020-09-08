using System.IO;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers
{
    public class EnsureValidationErrorCodesAreUniqueTests : DiagnosticVerifier
    {
        [Fact]
        public async Task Globally_unique_error_codes_are_accepted()
        {
            var source = await File.ReadAllTextAsync("TestSamples/EnsureValidationErrorCodesAreUnique_Unique_codes.cs");
            await VerifyDiagnostics(source);
        }

        [Fact]
        public async Task Duplicating_error_codes_are_rejected()
        {
            var source = await File.ReadAllTextAsync("TestSamples/EnsureValidationErrorCodesAreUnique_Duplicating_codes.cs");
            await VerifyDiagnostics(
                source,
                new DiagnosticResult(DiagnosticsIds.ErrorCodesShouldBeUnique, 8, 29));
        }

        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new EnsureValidationErrorCodesAreUnique();
        }
    }
}
