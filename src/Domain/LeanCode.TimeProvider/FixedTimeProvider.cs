using System;
using System.Threading;

namespace LeanCode.TimeProvider
{
    public sealed class FixedTimeProvider : ITimeProvider
    {
        public static readonly FixedTimeProvider SharedInstance = new FixedTimeProvider();

        private AsyncLocal<DateTime> savedTime = new AsyncLocal<DateTime>();

        /// <summary>
        /// Gets or sets the time for <b>current async context</b>.
        /// </summary>
        public DateTime Now
        {
            get => savedTime.Value;
            set => savedTime.Value = value;
        }

        private FixedTimeProvider() { }

        /// <summary>
        /// Sets this provider as current and updates time for current async context.
        /// </summary>
        public static void SetTo(DateTime time)
        {
            Time.UseTimeProvider(SharedInstance);
            SharedInstance.Now = time;
        }
    }
}
