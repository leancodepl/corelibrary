using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Security;
using LeanCode.DomainModels.Model;
using LeanCode.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App;

[AuthorizeWhenHasAnyOf("user")]
public class SampleQuery : IQuery<SampleQuery.Result?>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1034", Justification = "Better design.")]
    public sealed class Result
    {
        public string? Data { get; set; }
    }
}

public class SampleQueryHandler : IQueryHandler<AppContext, SampleQuery, SampleQuery.Result?>
{
    private readonly TestDbContext dbContext;

    public SampleQueryHandler(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<SampleQuery.Result?> ExecuteAsync(AppContext context, SampleQuery query)
    {
        // This will return `null`, but the query will be executed
        var data = await dbContext.Entities.Select(l => l.Value).FirstOrDefaultAsync();
        return new SampleQuery.Result { Data = $"{context.UserId}-{data}" };
    }
}

public sealed class AppContext : ISecurityContext
{
    private IPipelineScope? scope;
    private ClaimsPrincipal? user;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2201", Justification = "Expected behavior.")]
    IPipelineScope IPipelineContext.Scope
    {
        get => scope ?? throw new NullReferenceException();
        set => scope = value;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2201", Justification = "Expected behavior.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1065", Justification = "Expected behavior.")]
    public ClaimsPrincipal User
    {
        get => user ?? throw new NullReferenceException();
        set => user = value;
    }

    public CancellationToken CancellationToken { get; }

    public Guid UserId { get; }

    public AppContext(ClaimsPrincipal user, Guid userId, CancellationToken cancellationToken)
    {
        User = user;
        UserId = userId;
        CancellationToken = cancellationToken;
    }

    public static AppContext FromHttp(HttpContext context)
    {
        _ = Guid.TryParse(context.User?.FindFirst("sub")?.Value, out var uid);
        return new AppContext(context.User!, uid, context.RequestAborted);
    }
}
