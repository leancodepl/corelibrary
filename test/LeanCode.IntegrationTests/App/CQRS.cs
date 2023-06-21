using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.CQRS.Execution;
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

public class SampleQueryHandler : IQueryHandler<SampleQuery, SampleQuery.Result?>
{
    private readonly TestDbContext dbContext;

    public SampleQueryHandler(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<SampleQuery.Result?> ExecuteAsync(HttpContext context, SampleQuery query)
    {
        // This will return `null`, but the query will be executed
        var data = await dbContext.Entities.Select(l => l.Value).FirstOrDefaultAsync();
        return new SampleQuery.Result { Data = $"{context.User.Identity?.Name}-{data}" };
    }
}
