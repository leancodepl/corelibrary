using LeanCode.CodeAnalysis.Analyzers;
using LeanCode.CodeAnalysis.CodeFixProviders;
using LeanCode.CodeAnalysis.Tests.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace LeanCode.CodeAnalysis.Tests.CodeActions;

public class FixCQRSHandlerNamingCodeActionTests : CodeFixVerifier
{
    [Fact]
    public async Task Renames_command_handlers_not_following_naming_convention()
    {
        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class WrongCommandHandlerName : ICommandHandler<FirstCommand>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<WrongCommandHandlerName>();

    public WrongCommandHandlerName() { }

    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class FirstCommandCH : ICommandHandler<FirstCommand>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FirstCommandCH>();

    public FirstCommandCH() { }

    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Renames_query_handlers_not_following_naming_convention()
    {
        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class WrongQueryHandlerName : IQueryHandler<FirstQuery, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<WrongQueryHandlerName>();

    public WrongQueryHandlerName() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class FirstQueryQH : IQueryHandler<FirstQuery, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FirstQueryQH>();

    public FirstQueryQH() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Renames_operation_handlers_not_following_naming_convention()
    {
        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class WrongOperationHandlerName : IOperationHandler<FirstOperation, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<WrongOperationHandlerName>();

    public WrongOperationHandlerName() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class FirstOperationOH : IOperationHandler<FirstOperation, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<FirstOperationOH>();

    public FirstOperationOH() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    [Fact]
    public async Task Renames_handlers_to_multiple_contracts_not_following_naming_convention()
    {
        var source =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class MultipleQueries : IQueryHandler<FirstQuery, int>, IQueryHandler<SecondQuery, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<MultipleQueries>();

    public MultipleQueries() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<int> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;
using Serilog;

namespace Test;

public class MultipleQueriesQH : IQueryHandler<FirstQuery, int>, IQueryHandler<SecondQuery, int>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<MultipleQueriesQH>();

    public MultipleQueriesQH() { }

    public Task<int> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<int> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
}";

        var fixes = new[] { "Fix CQRS handler name" };
        await VerifyCodeFix(source, expected, fixes, 0);
    }

    protected override CodeFixProvider GetCodeFixProvider()
    {
        return new FixCQRSHandlerNamingCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
    {
        return new EnsureCQRSHandlersFollowNamingConvention();
    }
}
