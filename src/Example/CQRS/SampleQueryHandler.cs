using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : IQueryHandler<SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> ExecuteAsync(SampleQuery query)
        {
            return Task.FromResult(new SampleQuery.Result("LeanCode"));
        }
    }
}
