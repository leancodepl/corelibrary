using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class AddCommandValidatorCodeActionTests : CodeFixVerifier
{
    [Fact]
    public async Task Creates_validator()
    {
        var source =
            @"
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace Test
{
    public class Command : ICommand { }
    public class Handler : ICommandHandler<Command>
    {
        public Task ExecuteAsync(HttpContext ctx, Command cmd) => Task.CompletedTask;
    }
}";

        var expected =
            @"
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace Test
{
    public class Command : ICommand { }

    public class CommandCV : AbstractValidator<Command>
    {
        public CommandCV()
        {
        }
    }

    public class Handler : ICommandHandler<Command>
    {
        public Task ExecuteAsync(HttpContext ctx, Command cmd) => Task.CompletedTask;
    }
}";
        var fixes = new[] { "Add CommandValidator" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Creates_validator_and_namespace_if_missing()
    {
        var source =
            @"
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace Test
{
    public class Command : ICommand { }
    public class Handler : ICommandHandler<Command>
    {
        public Task ExecuteAsync(HttpContext ctx, Command cmd) => Task.CompletedTask;
    }
}";

        var expected =
            @"
using System.Threading.Tasks;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using Microsoft.AspNetCore.Http;

namespace Test
{
    public class Command : ICommand { }

    public class CommandCV : AbstractValidator<Command>
    {
        public CommandCV()
        {
        }
    }

    public class Handler : ICommandHandler<Command>
    {
        public Task ExecuteAsync(HttpContext ctx, Command cmd) => Task.CompletedTask;
    }
}";
        var fixes = new[] { "Add CommandValidator" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new AddCommandValidatorCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new SuggestCommandsHaveValidators();
    }
}
