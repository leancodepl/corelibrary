using System;
using System.Threading;

namespace LeanCode.Time;

public sealed class FixedTimeProvider : ITimeProvider
{
    private static readonly FixedTimeProvider SharedInstance = new();

    private readonly AsyncLocal<DateTimeOffset?> savedTime = new();

    /// <summary>
    /// Gets the time for <b>current async context</b> as UTC <see cref="DateTime"/>.
    /// </summary>
    public DateTime Now => savedTime.Value?.UtcDateTime ?? DateTime.UtcNow;

    /// <summary>
    /// Gets the time for <b>current async context</b> as <see cref="DateTimeOffset"/>.
    /// </summary>
    public DateTimeOffset NowWithOffset => savedTime.Value ?? DateTimeOffset.UtcNow;

    private FixedTimeProvider() { }

    /// <summary>
    /// Sets this provider as current and updates time for current async context.
    /// </summary>
    /// <remarks>
    /// Argument cannot have <see cref="DateTimeKind"/> of <see cref="DateTimeKind.Local"/>
    /// and <see cref="DateTimeKind.Unspecified"/> is treated as <see cref="DateTimeKind.Utc"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <see cref="DateTimeKind"/> of the argument is <see cref="DateTimeKind.Local"/>.
    /// </exception>
    public static void SetTo(DateTime time)
    {
        if (time.Kind == DateTimeKind.Local)
        {
            throw new InvalidOperationException(
                "Cannot assign local DateTime, use SetTo(DateTimeOffset) overload with correct offset instead."
            );
        }

        TimeProvider.Use(SharedInstance);

        SharedInstance.savedTime.Value = new DateTimeOffset(time, TimeSpan.Zero);
    }

    /// <summary>
    /// Sets this provider as current and updates time for current async context.
    /// </summary>
    public static void SetTo(DateTimeOffset time)
    {
        TimeProvider.Use(SharedInstance);

        SharedInstance.savedTime.Value = time;
    }
}
