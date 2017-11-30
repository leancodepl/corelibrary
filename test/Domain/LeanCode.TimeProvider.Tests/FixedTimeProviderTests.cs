using System;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.TimeProvider.Tests
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

        [Fact]
        public void Check_the_value_sync()
        {
            var provider = FixedTimeProvider.SharedInstance;

            Time.UseTimeProvider(provider);
            provider.Now = expectedTime;

            for (int i = 0; i < Iterations; i++)
            {
                Assert.Equal(Time.Now, expectedTime);
            }
        }

        [Fact]
        public async Task Check_the_value_async()
        {
            var provider = FixedTimeProvider.SharedInstance;

            Time.UseTimeProvider(provider);
            provider.Now = expectedTime;

            for (int i = 0; i < DelayIterations; i++)
            {
                Assert.Equal(Time.Now, expectedTime);
                await Task.Delay(Delay);
            }
        }
    }

    public class FixedTimeProviderTests1 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests1()
            : base(new DateTime(2017, 11, 30, 15, 45, 0))
        { }
    }

    public class FixedTimeProviderTests2 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests2()
            : base(new DateTime(2016, 11, 30, 0, 0, 0))
        { }
    }

    public class FixedTimeProviderTests3 : FixedTimeProviderTestsBase
    {
        public FixedTimeProviderTests3()
            : base(new DateTime(2015, 11, 30, 6, 0, 0))
        { }
    }
}
