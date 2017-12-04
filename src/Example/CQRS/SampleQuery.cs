using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class SampleQuery : IRemoteQuery<LocalContext, SampleQuery.Result>
    {
        public sealed class Result
        {
            public string Name { get; }

            public Result(string name)
            {
                Name = name;
            }
        }
    }
}
