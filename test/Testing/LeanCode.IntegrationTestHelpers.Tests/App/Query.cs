using System.Linq;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class Query : IRemoteQuery<string?>
    {
        public int Id { get; set; }
    }

    public class QueryQH : IQueryHandler<Context, Query, string?>
    {
        private readonly TestDbContext dbContext;

        public QueryQH(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<string?> ExecuteAsync(Context context, Query query)
        {
            return dbContext.Entities.Where(e => e.Id == query.Id).Select(e => e.Data).FirstOrDefaultAsync();
        }
    }
}
