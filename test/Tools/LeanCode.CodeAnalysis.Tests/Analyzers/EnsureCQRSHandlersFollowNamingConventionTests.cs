using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnsureCQRSHandlersFollowNamingConventionTests : DiagnosticVerifier
{
    [Theory]
    [InlineData("command")]
    [InlineData("query")]
    [InlineData("operation")]
    public async Task CQRS_handlers_following_naming_convention_are_accepted(string cqrsType)
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Accepted_{cqrsType}_handlers.cs");
        await VerifyDiagnostics(source);
    }

    [Theory]
    [InlineData("command", DiagnosticsIds.CommandHandlersShouldFollowNamingConvention)]
    [InlineData("query", DiagnosticsIds.QueryHandlersShouldFollowNamingConvention)]
    [InlineData("operation", DiagnosticsIds.OperationHandlersShouldFollowNamingConvention)]
    public async Task CQRS_handlers_not_following_naming_convention_are_rejected(string cqrsType, string diagnosticId)
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Rejected_{cqrsType}_handlers.cs");

        var diags = new[]
        {
            new DiagnosticResult(diagnosticId, 6, 13),
            new DiagnosticResult(diagnosticId, 11, 13)
        };

        await VerifyDiagnostics(source, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCQRSHandlersFollowNamingConvention();
    }
}
