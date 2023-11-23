using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class FixCQRSHandlerNamespaceTests : CodeFixVerifier
{
    [Fact]
    public async Task Moves_command_handlers_to_matching_contracts_namespace()
    {
        var contractSource =
            @"
using LeanCode.Contracts;

namespace Test.Contracts.Directory.Subdirectory;

public class Command : ICommand { }
";

        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Wrong.Namespace;

public class CommandCH : ICommandHandler<Command>
{
    public CommandCH() { }

    public Task ExecuteAsync(HttpContext context, Command command) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Directory.Subdirectory;

public class CommandCH : ICommandHandler<Command>
{
    public CommandCH() { }

    public Task ExecuteAsync(HttpContext context, Command command) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler namespace" };
        await VerifyCodeFix(source, expected, fixes, 0, false, contractSource);
    }

    [Fact]
    public async Task Moves_query_handlers_to_matching_contracts_namespace()
    {
        var contractSource =
            @"
using LeanCode.Contracts;

namespace Test.Contracts.Directory.Subdirectory;

public class Query : IQuery<bool> { }
";

        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Wrong.Namespace;

public class QueryQH : IQueryHandler<Query, bool>
{
    public QueryQH() { }

    public Task ExecuteAsync(HttpContext context, Query query) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Directory.Subdirectory;

public class QueryQH : IQueryHandler<Query, bool>
{
    public QueryQH() { }

    public Task ExecuteAsync(HttpContext context, Query query) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler namespace" };
        await VerifyCodeFix(source, expected, fixes, 0, false, contractSource);
    }

    [Fact]
    public async Task Moves_operation_handlers_to_matching_contracts_namespace()
    {
        var contractSource =
            @"
using LeanCode.Contracts;

namespace Test.Contracts.Directory.Subdirectory;

public class Operation : IOperation<bool> { }
";

        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Wrong.Namespace;

public class OperationOH : IOperationHandler<Operation, bool>
{
    public OperationOH() { }

    public Task ExecuteAsync(HttpContext context, Operation operation) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Test.Contracts.Directory.Subdirectory;

namespace Test.CQRS.Directory.Subdirectory;

public class OperationOH : IOperationHandler<Operation, bool>
{
    public OperationOH() { }

    public Task ExecuteAsync(HttpContext context, Operation operation) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler namespace" };
        await VerifyCodeFix(source, expected, fixes, 0, false, contractSource);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new FixCQRSHandlerNamespaceCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCQRSHandlersAreInProperNamespace();
    }
}
