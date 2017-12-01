using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : IQueryHandler<VoidContext, SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> ExecuteAsync(VoidContext _, SampleQuery query)
        {
            return Task.FromResult(new SampleQuery.Result("LeanCode"));
        }
    }
}
