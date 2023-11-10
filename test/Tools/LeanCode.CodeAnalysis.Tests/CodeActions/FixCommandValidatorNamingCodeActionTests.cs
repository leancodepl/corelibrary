using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class FixCommandValidatorNamingCodeActionTests : CodeFixVerifier
{
    [Fact]
    public async Task Renames_command_validators_not_following_naming_convention()
    {
        var source =
            @"
using FluentValidation;
using LeanCode.Contracts;

namespace Test;

public class Command : ICommand { }

public class WrongName : AbstractValidator<Command>
{
    public WrongName() { }
}";

        var expected =
            @"
using FluentValidation;
using LeanCode.Contracts;

namespace Test;

public class Command : ICommand { }

public class CommandCV : AbstractValidator<Command>
{
    public CommandCV() { }
}";

        var fixes = new[] { "Fix command validator name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new FixCommandValidatorNamingCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCommandValidatorsFollowNamingConvention();
    }
}
