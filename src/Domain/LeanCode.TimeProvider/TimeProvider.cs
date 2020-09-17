using System;

namespace LeanCode.Time
{
    public static class TimeProvider
    {
        private static ITimeProvider timeProvider = new UtcTimeProvider();

        public static DateTime Now => timeProvider.Now;
        public static DateTimeOffset NowWithOffset => timeProvider.NowWithOffset;

        public static void Use(ITimeProvider newProvider) =>
            timeProvider = newProvider;
    }
}
