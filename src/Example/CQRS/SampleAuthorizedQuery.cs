using LeanCode.CQRS;
using LeanCode.CQRS.Security;
using LeanCode.Example.Security;

namespace LeanCode.Example.CQRS
{
    [AuthorizeWhenHasAnyOf(Permissions.View)]
    public class SampleAuthorizedQuery : IRemoteQuery<SampleAuthorizedQuery.Result>
    {
        public sealed class Result
        {
            [CanBeNull]
            public string Name { get; }

            public Result(string name)
            {
                Name = name;
            }
        }
    }
}
