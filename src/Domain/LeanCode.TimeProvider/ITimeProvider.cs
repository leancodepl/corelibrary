using System;

namespace LeanCode.TimeProvider
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
