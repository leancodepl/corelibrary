using LeanCode.CQRS;
using LeanCode.CQRS.Security;
using LeanCode.Example.Security;

namespace LeanCode.Example.CQRS
{
    [AuthorizeWhenHasAnyOf(Permissions.View)]
    public class SampleAuthorizedQuery : IRemoteQuery<LocalContext, SampleAuthorizedQuery.Result>
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
