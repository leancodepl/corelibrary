using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleAuthorizedQueryHandler : IQueryHandler<LocalContext, SampleAuthorizedQuery, SampleAuthorizedQuery.Result>
    {
        public Task<SampleAuthorizedQuery.Result> ExecuteAsync(LocalContext context, SampleAuthorizedQuery query)
        {
            return Task.FromResult(new SampleAuthorizedQuery.Result("LeanCode"));
        }
    }
}
