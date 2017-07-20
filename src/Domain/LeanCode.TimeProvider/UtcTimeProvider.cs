using System;

namespace LeanCode.TimeProvider
{
    public sealed class UtcTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
