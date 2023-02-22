using System.IO;
using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnsureCommandsQueriesAndOperationsHaveAuthorizersTests : DiagnosticVerifier
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
    public async Task Operations_with_authorization_attributes_are_accepted()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Accepted_operations.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Commands_without_authorization_are_rejected()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Rejected_commands.cs");
        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.CommandsShouldHaveAuthorizers, 4, 13),
            new DiagnosticResult(DiagnosticsIds.CommandsShouldHaveAuthorizers, 6, 13),
        };

        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Queries_without_authorization_are_rejected()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Rejected_queries.cs");
        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.QueriesShouldHaveAuthorizers, 4, 13),
            new DiagnosticResult(DiagnosticsIds.QueriesShouldHaveAuthorizers, 6, 13),
        };

        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Operations_without_authorization_are_rejected()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Rejected_operations.cs");
        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.OperationsShouldHaveAuthorizers, 4, 13),
            new DiagnosticResult(DiagnosticsIds.OperationsShouldHaveAuthorizers, 6, 13),
        };

        await VerifyDiagnostics(source, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCommandsQueriesAndOperationsHaveAuthorizers();
    }
}
