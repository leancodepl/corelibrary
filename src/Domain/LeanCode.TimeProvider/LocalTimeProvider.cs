using System;

namespace LeanCode.TimeProvider
{
    [Obsolete("Using LocalTimeProvider is considered anti-pattern.")]
    public sealed class LocalTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
