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
        var source = await File.ReadAllTextAsync("TestSamples/Accepted/Contracts/Commands.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Queries_with_authorization_attributes_are_accepted()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Accepted/Contracts/Queries.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Operations_with_authorization_attributes_are_accepted()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Accepted/Contracts/Operations.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Commands_without_authorization_are_rejected()
    {
        var source = await File.ReadAllTextAsync("TestSamples/Rejected/Contracts/Commands.cs");
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
        var source = await File.ReadAllTextAsync("TestSamples/Rejected/Contracts/Queries.cs");
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
        var source = await File.ReadAllTextAsync("TestSamples/Rejected/Contracts/Operations.cs");
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
