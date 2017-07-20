using System;

namespace LeanCode.TimeProvider
{
    public sealed class FixedTimeProvider : ITimeProvider
    {
        public DateTime Now { get; }

        public FixedTimeProvider(DateTime now)
        {
            Now = now;
        }
    }
}
