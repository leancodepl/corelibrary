using System.Threading.Tasks;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : IQueryHandler<object, SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> ExecuteAsync(object _, SampleQuery query)
        {
            return Task.FromResult(new SampleQuery.Result("LeanCode"));
        }
    }
}
