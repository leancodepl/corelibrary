using System;
using System.Threading.Tasks;
using GreenPipes.Internals.Extensions;
using LeanCode.DomainModels.MassTransitRelay.Testing;
using MassTransit;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.Testing
{
    public class ResettableBusActivityMonitorTests
    {
        private readonly ResettableBusActivityMonitor monitor = new ResettableBusActivityMonitor(TimeSpan.FromSeconds(0.2));

        [Fact]
        public void Is_inactive_by_default()
        {
            Assert.True(monitor.AwaitBusInactivity().IsCompleted);
        }

        [Fact]
        public void Makes_itself_inactive_after_first_new_consumer()
        {
            _ = ((IConsumeObserver)monitor).PreConsume<object>(null!);

            Assert.False(monitor.AwaitBusInactivity().IsCompleted);
        }

        [Fact]
        public void Makes_itself_inactive_after_first_new_receiver()
        {
            _ = ((IReceiveObserver)monitor).PreReceive(null!);

            Assert.False(monitor.AwaitBusInactivity().IsCompleted);
        }

        [Fact]
        public void Does_not_set_itself_right_after_cosumers_stabilize()
        {
            _ = ((IConsumeObserver)monitor).PreConsume<object>(null!);
            _ = ((IConsumeObserver)monitor).PostConsume<object>(null!);

            Assert.False(monitor.AwaitBusInactivity().IsCompleted);
        }

        [Fact]
        public async Task Sets_itself_after_the_delay()
        {
            await ((IConsumeObserver)monitor).PreConsume<object>(null!);
            await ((IConsumeObserver)monitor).PostConsume<object>(null!);

            await monitor.AwaitBusInactivity().OrTimeout(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Sets_the_inactivity_after_equal_pre_and_post_consumes()
        {
            const int Repeat = 10;
            for (var i = 0; i < Repeat; i++)
            {
                await ((IConsumeObserver)monitor).PreConsume<object>(null!);
            }

            for (var i = 0; i < Repeat; i++)
            {
                await ((IConsumeObserver)monitor).PostConsume<object>(null!);
            }

            await monitor.AwaitBusInactivity().OrTimeout(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task If_the_calls_dont_compensate_the_inactivity_is_not_set()
        {
            const int Repeat = 10;
            for (var i = 0; i < Repeat; i++)
            {
                await ((IConsumeObserver)monitor).PreConsume<object>(null!);
            }

            for (var i = 0; i < Repeat - 1; i++)
            {
                await ((IConsumeObserver)monitor).PostConsume<object>(null!);
            }

            await Assert.ThrowsAsync<TimeoutException>(() => monitor.AwaitBusInactivity().OrTimeout(TimeSpan.FromSeconds(1)));
        }
    }
}
