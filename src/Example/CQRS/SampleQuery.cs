using LeanCode.CQRS;
using LeanCode.CQRS.Security;

namespace LeanCode.Example.CQRS
{
    [AllowUnauthorized]

    public class SampleQuery : IRemoteQuery<SampleQuery.Result>
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
