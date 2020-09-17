using System;
using System.Threading.Tasks;
using LeanCode.Test.Helpers;
using Xunit;

namespace LeanCode.Time.Tests
{
    public abstract class FixedTimeProviderTestsBase
    {
        private const int Iterations = 10_000;

        private const int DelayIterations = 100;
        private const int Delay = 10;

        private readonly DateTime expectedTime;

        protected FixedTimeProviderTestsBase(DateTime expectedTime)
        {
            this.expectedTime = expectedTime;
        }

        [LongRunningFact]
        public void Check_the_value_sync()
        {
            FixedTimeProvider.SetTo(expectedTime);

            for (int i = 0; i < Iterations; i++)
            {
                Assert.Equal(expectedTime, TimeProvider.Now);
            }
        }

        [LongRunningFact]
        public async Task Check_the_value_async()
        {
            FixedTimeProvider.SetTo(expectedTime);

            for (int i = 0; i < DelayIterations; i++)
            {
                Assert.Equal(expectedTime, TimeProvider.Now);
                await Task.Delay(Delay);
            }
        }

        [Fact]
        public void SetTo_overload_for_DateTime_correctly_alters_return_value_of_NowWithOffset()
        {
            FixedTimeProvider.SetTo(expectedTime);

            var expectedTimeWithOffset = new DateTimeOffset(expectedTime, TimeSpan.Zero);

            Assert.Equal(expectedTimeWithOffset, TimeProvider.NowWithOffset);
        }
    }

    public class FixedTimeProviderTests1 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests1()
            : base(new DateTime(2017, 11, 30, 15, 45, 0)) { }
    }

    public class FixedTimeProviderTests2 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests2()
            : base(new DateTime(2016, 11, 30, 0, 0, 0)) { }
    }

    public class FixedTimeProviderTests3 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests3()
            : base(new DateTime(2015, 11, 30, 6, 0, 0)) { }
    }

    public class FixedTimeProviderTests
    {
        [Fact]
        public void Attempt_to_assign_local_date_time_throws()
        {
            var date = new DateTime(2019, 2, 25, 0, 0, 0, DateTimeKind.Local);

            Assert.Throws<InvalidOperationException>(() =>
            {
                FixedTimeProvider.SetTo(date);
            });
        }
    }
}
