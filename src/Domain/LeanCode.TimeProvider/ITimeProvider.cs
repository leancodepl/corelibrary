using System;

namespace LeanCode.Time;

public interface ITimeProvider
{
    DateTime Now { get; }
    DateTimeOffset NowWithOffset { get; }
}
