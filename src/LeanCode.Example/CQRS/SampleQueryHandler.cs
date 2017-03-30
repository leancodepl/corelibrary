using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : IQueryHandler<SampleQuery, SampleQuery.Result>
    {
        public SampleQuery.Result Execute(SampleQuery query)
        {
            return new SampleQuery.Result("LeanCode");
        }
    }
}
