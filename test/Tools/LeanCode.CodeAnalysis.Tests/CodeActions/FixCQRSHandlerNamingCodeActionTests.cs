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
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class WrongCommandHandlerName : ICommandHandler<FirstCommand>
{
    private readonly ILogger<WrongCommandHandlerName> logger;

    public WrongCommandHandlerName(ILogger<WrongCommandHandlerName> logger)
    {
        this.logger = logger;
    }

    public Task ExecuteAsync(HttpContext context, FirstCommand command) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class FirstCommandCH : ICommandHandler<FirstCommand>
{
    private readonly ILogger<FirstCommandCH> logger;

    public FirstCommandCH(ILogger<FirstCommandCH> logger)
    {
        this.logger = logger;
    }

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
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class WrongQueryHandlerName : IQueryHandler<FirstQuery, bool>
{
    private readonly ILogger<FirstCommandCH> logger;

    public WrongQueryHandlerName(ILogger<FirstCommandCH> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class FirstQueryQH : IQueryHandler<FirstQuery, bool>
{
    private readonly ILogger<FirstCommandCH> logger;

    public FirstQueryQH(ILogger<FirstCommandCH> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();
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
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class WrongOperationHandlerName : IOperationHandler<FirstOperation, bool>
{
    private readonly ILogger<WrongOperationHandlerName> logger;

    public WrongOperationHandlerName(ILogger<WrongOperationHandlerName> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class FirstOperationOH : IOperationHandler<FirstOperation, bool>
{
    private readonly ILogger<FirstOperationOH> logger;

    public FirstOperationOH(ILogger<FirstOperationOH> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstOperation operation) => throw new NotImplementedException();
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
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class MultipleQueries : IQueryHandler<FirstQuery, bool>, IQueryHandler<SecondQuery, bool>
{
    private readonly ILogger<MultipleQueries> logger;

    public MultipleQueries(ILogger<MultipleQueries> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<bool> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
}";

        var expected =
            @"
using LeanCode.CQRS.Execution;
using Leancode.Logging;
using Microsoft.AspNetCore.Http;
using LeanCode.CodeAnalysis.Tests.Data;

namespace Test;

public class MultipleQueriesQH : IQueryHandler<FirstQuery, bool>, IQueryHandler<SecondQuery, bool>
{
    private readonly ILogger<MultipleQueriesQH> logger;

    public MultipleQueriesQH(ILogger<MultipleQueriesQH> logger)
    {
        this.logger = logger;
    }

    public Task<bool> ExecuteAsync(HttpContext context, FirstQuery query) => throw new NotImplementedException();

    public Task<bool> ExecuteAsync(HttpContext context, SecondQuery query) => throw new NotImplementedException();
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
