using System.Linq;
using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class Query : IQuery<string?>
{
    public int Id { get; set; }
}

public class QueryQH : IQueryHandler<Query, string?>
{
    private readonly TestDbContext dbContext;

    public QueryQH(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<string?> ExecuteAsync(HttpContext context, Query query)
    {
        return dbContext.Entities.Where(e => e.Id == query.Id).Select(e => e.Data).FirstOrDefaultAsync();
    }
}
