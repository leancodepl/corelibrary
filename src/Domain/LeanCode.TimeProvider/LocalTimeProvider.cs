using System;

namespace LeanCode.TimeProvider
{
    public sealed class LocalTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
