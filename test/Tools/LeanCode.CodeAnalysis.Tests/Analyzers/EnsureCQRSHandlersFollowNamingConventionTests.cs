using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.Analyzers;

public class EnsureCQRSHandlersFollowNamingConventionTests : DiagnosticVerifier
{
    [Theory]
    [InlineData("Command")]
    [InlineData("Query")]
    [InlineData("Operation")]
    public async Task CQRS_handlers_following_naming_convention_are_accepted(string cqrsType)
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Accepted/CQRS/{cqrsType}_handlers.cs");
        await VerifyDiagnostics(source);
    }

    [Fact]
    public async Task Command_handlers_not_following_naming_convention_are_rejected()
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Rejected/CQRS/Command_handlers.cs");

        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.CommandHandlersShouldFollowNamingConvention, 6, 13),
            new DiagnosticResult(DiagnosticsIds.CommandHandlersShouldFollowNamingConvention, 11, 13)
        };

        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Query_handlers_not_following_naming_convention_are_rejected()
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Rejected/CQRS/Query_handlers.cs");

        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.QueryHandlersShouldFollowNamingConvention, 6, 13),
            new DiagnosticResult(DiagnosticsIds.QueryHandlersShouldFollowNamingConvention, 11, 13)
        };

        await VerifyDiagnostics(source, diags);
    }

    [Fact]
    public async Task Operation_handlers_not_following_naming_convention_are_rejected()
    {
        var source = await File.ReadAllTextAsync($"TestSamples/Rejected/CQRS/Operation_handlers.cs");

        var diags = new[]
        {
            new DiagnosticResult(DiagnosticsIds.OperationHandlersShouldFollowNamingConvention, 6, 13),
            new DiagnosticResult(DiagnosticsIds.OperationHandlersShouldFollowNamingConvention, 12, 13)
        };

        await VerifyDiagnostics(source, diags);
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCQRSHandlersFollowNamingConvention();
    }
}
