using LeanCode.CQRS;
using LeanCode.CQRS.Security;

namespace LeanCode.Example.CQRS
{
    [AllowUnauthorized]
    public class SampleCommand : IRemoteCommand
    {
        public string Name { get; }

        public SampleCommand(string name)
        {
            Name = name;
        }
    }
}
