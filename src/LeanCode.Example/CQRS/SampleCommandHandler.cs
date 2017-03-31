using System;
using LeanCode.CQRS;
using LeanCode.DomainModels.Model;

namespace LeanCode.Example.CQRS
{
    public class SampleCommandHandler : SyncCommandHandler<SampleCommand>
    {
        private static readonly Random rnd = new Random();

        public override void Execute(SampleCommand command)
        {
            var d = rnd.NextDouble();
            if (d < 0.5)
            {
                throw new InvalidOperationException("Randomness isn't on our side...");
            }

            DomainEvents.Raise(new SampleEvent());
        }
    }
}
