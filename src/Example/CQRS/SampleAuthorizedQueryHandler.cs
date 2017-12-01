using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleAuthorizedQueryHandler : IQueryHandler<VoidContext, SampleAuthorizedQuery, SampleAuthorizedQuery.Result>
    {
        public Task<SampleAuthorizedQuery.Result> ExecuteAsync(VoidContext context, SampleAuthorizedQuery query)
        {
            return Task.FromResult(new SampleAuthorizedQuery.Result("LeanCode"));
        }
    }
}
