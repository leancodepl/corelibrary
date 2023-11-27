using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnsureCQRSHandlersAreInProperNamespaceTests : DiagnosticVerifier
{
    [Theory]
    [InlineData("Command")]
    [InlineData("Query")]
    [InlineData("Operation")]
    public async Task CQRS_handlers_in_proper_namespace_are_accepted(string cqrsType)
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Accepted/CQRS/{cqrsType}_handlers.cs");
        await VerifyDiagnostics(source);
    }

    [Theory]
    [InlineData("Commands", "Command_handlers")]
    [InlineData("Queries", "Query_handlers")]
    [InlineData("Operations", "Operation_handlers")]
    public async Task CQRS_handlers_not_in_proper_namespace_are_rejected(string contractsFile, string handlersFile)
    {
        var contractSource = await File.ReadAllTextAsync($"TestSamples/Accepted/Contracts/{contractsFile}.cs");
        var handlerSource = await File.ReadAllTextAsync($"TestSamples/Rejected/CQRS/{handlersFile}.cs");

        var diags = new[] { new DiagnosticResult(DiagnosticsIds.CQRSHandlersShouldBeInProperNamespace, 4, 0), };

        await VerifyDiagnostics(new[] { contractSource, handlerSource }, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCQRSHandlersAreInProperNamespace();
    }
}
