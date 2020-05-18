using System.IO;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers
{
    public class EnsureCommandsAndQueriesHaveAuthorizersTests : DiagnosticVerifier
    {
        [Fact]
        public async Task Commands_with_authorization_attributes_are_accepted()
        {
            var source = await File.ReadAllTextAsync("TestSamples/Accepted_commands.cs");
            await VerifyDiagnostics(source);
        }

        [Fact]
        public async Task Queries_with_authorization_attributes_are_accepted()
        {
            var source = await File.ReadAllTextAsync("TestSamples/Accepted_queries.cs");
            await VerifyDiagnostics(source);
        }

        [Fact]
        public async Task Commands_without_authorization_are_rejected()
        {
            var source = await File.ReadAllTextAsync("TestSamples/Rejected_commands.cs");
            var diags = new[]
            {
                new DiagnosticResult(DiagnosticsIds.CommandsShouldHaveAuthorizers, 4, 17),
                new DiagnosticResult(DiagnosticsIds.CommandsShouldHaveAuthorizers, 6, 17),
            };

            await VerifyDiagnostics(source, diags);
        }

        [Fact]
        public async Task Queries_without_authorization_are_rejected()
        {
            var source = await File.ReadAllTextAsync("TestSamples/Rejected_queries.cs");
            var diags = new[]
            {
                new DiagnosticResult(DiagnosticsIds.QueriesShouldHaveAuthorizers, 4, 17),
                new DiagnosticResult(DiagnosticsIds.QueriesShouldHaveAuthorizers, 6, 17),
            };

            await VerifyDiagnostics(source, diags);
        }

        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new EnsureCommandsAndQueriesHaveAuthorizers();
        }
    }
}
