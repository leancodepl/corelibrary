using System;
using System.Threading.Tasks;
using LeanCode.Test.Helpers;
using Xunit;

namespace LeanCode.Time.Tests
{
    public abstract class FixedTimeProviderOffsetTestsBase
    {
        private const int Iterations = 10_000;

        private const int DelayIterations = 100;
        private const int Delay = 10;

        private readonly DateTimeOffset expectedTime;

        protected FixedTimeProviderOffsetTestsBase(DateTimeOffset expectedTime)
        {
            this.expectedTime = expectedTime;
        }

        [LongRunningFact]
        public void Check_the_value_sync()
        {
            FixedTimeProvider.SetTo(expectedTime);

            for (var i = 0; i < Iterations; i++)
            {
                Assert.Equal(expectedTime, TimeProvider.NowWithOffset);
            }
        }

        [LongRunningFact]
        public async Task Check_the_value_async()
        {
            FixedTimeProvider.SetTo(expectedTime);

            for (var i = 0; i < DelayIterations; i++)
            {
                Assert.Equal(expectedTime, TimeProvider.NowWithOffset);
                await Task.Delay(Delay);
            }
        }

        [Fact]
        public void SetTo_overload_for_DateTimeOffset_correctly_alters_return_value_of_Now()
        {
            FixedTimeProvider.SetTo(expectedTime);

            Assert.Equal(expectedTime.UtcDateTime, TimeProvider.Now);
        }
    }

    public class FixedTimeProviderOffsetTests1 : FixedTimeProviderOffsetTestsBase
    {
        public FixedTimeProviderOffsetTests1()
            : base(new DateTimeOffset(new DateTime(2017, 11, 30, 15, 45, 0, DateTimeKind.Utc))) { }
    }

    public class FixedTimeProviderOffsetTests2 : FixedTimeProviderOffsetTestsBase
    {
        public FixedTimeProviderOffsetTests2()
            : base(new DateTimeOffset(new DateTime(2016, 11, 30, 0, 0, 0, DateTimeKind.Utc))) { }
    }

    public class FixedTimeProviderOffsetTests3 : FixedTimeProviderOffsetTestsBase
    {
        public FixedTimeProviderOffsetTests3()
            : base(new DateTimeOffset(new DateTime(2015, 11, 30, 6, 0, 0, DateTimeKind.Local))) { }
    }
}
