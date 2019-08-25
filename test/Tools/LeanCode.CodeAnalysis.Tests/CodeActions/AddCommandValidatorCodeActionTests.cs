using System.Threading.Tasks;
using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions
{
    public class AddCommandValidatorCodeActionTests : CodeFixVerifier
    {
        [Fact]
        public async Task Creates_validator()
        {
            var source = @"
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

public class Context { }
public class Command : ICommand { }
public class Handler : ICommandHandler<Context, Command>
{
    public Task ExecuteAsync(Context ctx, Command cmd) => Task.CompletedTask;
}";

            var expected = @"
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

public class Context { }
public class Command : ICommand { }
public class CommandCV : ContextualValidator<Command>
{
    public CommandCV()
    {
    }
}
public class Handler : ICommandHandler<Context, Command>
{
    public Task ExecuteAsync(Context ctx, Command cmd) => Task.CompletedTask;
}";
            var fixes = new[] { "Add CommandValidator" };
            await VerifyCodeFix(source, expected, fixes, 0);
        }

        [Fact(Skip = "Failing on indentation and using directive placement")]
        public async Task Creates_validator_and_namespace_if_missing()
        {
            var source = @"
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

public class Context { }
public class Command : ICommand { }
public class Handler : ICommandHandler<Context, Command>
{
    public Task ExecuteAsync(Context ctx, Command cmd) => Task.CompletedTask;
}";

            var expected = @"
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;

public class Context { }
public class Command : ICommand { }
public class CommandCV : ContextualValidator<Command>
{
    public CommandCV()
    {
    }
}
public class Handler : ICommandHandler<Context, Command>
{
    public Task ExecuteAsync(Context ctx, Command cmd) => Task.CompletedTask;
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
}
