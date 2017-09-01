using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleAuthorizedQueryHandler : IQueryHandler<object, SampleAuthorizedQuery, SampleAuthorizedQuery.Result>
    {
        public Task<SampleAuthorizedQuery.Result> ExecuteAsync(object context, SampleAuthorizedQuery query)
        {
            return Task.FromResult(new SampleAuthorizedQuery.Result("LeanCode"));
        }
    }
}
