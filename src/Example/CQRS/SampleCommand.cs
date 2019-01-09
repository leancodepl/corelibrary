using LeanCode.CQRS;

namespace LeanCode.Example.CQRS
{
    public class SampleCommand : IRemoteCommand
    {
        public string Name { get; }

        public SampleCommand(string name)
        {
            Name = name;
        }
    }
}
