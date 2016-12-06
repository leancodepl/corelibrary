using System;

namespace LeanCode.TimeProvider
{
    public class FixedTimeProvider : ITimeProvider
    {
        public DateTime Now { get; }

        public FixedTimeProvider(DateTime now)
        {
            Now = now;
        }
    }
}
