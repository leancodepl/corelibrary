using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : SyncQueryHandler<SampleQuery, SampleQuery.Result>
    {
        public override SampleQuery.Result Execute(SampleQuery query)
        {
            return new SampleQuery.Result("LeanCode");
        }
    }
}
