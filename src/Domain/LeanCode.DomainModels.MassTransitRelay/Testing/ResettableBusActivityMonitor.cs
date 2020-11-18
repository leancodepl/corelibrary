using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing.Indicators;
using MassTransit.Util;

namespace LeanCode.DomainModels.MassTransitRelay.Testing
{
    public class ResettableBusActivityMonitor :
        IBusActivityMonitor,
        IReceiveObserver,
        IConsumeObserver,
        IDisposable
    {
        private readonly object mutex = new object();
        private readonly AsyncManualResetEvent inactive = new AsyncManualResetEvent(true);
        private readonly RollingTimer timer;

        private volatile int consumersInFlight;
        private volatile int receiversInFlight;

        public ResettableBusActivityMonitor(TimeSpan inactivityWaitTime)
        {
            timer = new RollingTimer(OnTimer, inactivityWaitTime);
        }

        public static ResettableBusActivityMonitor CreateFor(IBusControl bus, TimeSpan inactivityWaitTime)
        {
            var monitor = new ResettableBusActivityMonitor(inactivityWaitTime);
            bus.ConnectConsumeObserver(monitor);
            bus.ConnectReceiveObserver(monitor);
            return monitor;
        }

        public Task AwaitBusInactivity() => inactive.WaitAsync();
        public Task<bool> AwaitBusInactivity(TimeSpan timeout) => inactive.WaitAsync(timeout);
        public Task AwaitBusInactivity(CancellationToken cancellationToken) => inactive.WaitAsync(cancellationToken);

        Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
            where T : class
        {
            lock (mutex)
            {
                consumersInFlight--;
                CheckCondition();
            }

            return Task.CompletedTask;
        }

        Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context)
            where T : class
        {
            lock (mutex)
            {
                consumersInFlight--;
                CheckCondition();
            }

            return Task.CompletedTask;
        }

        Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context)
            where T : class
        {
            lock (mutex)
            {
                consumersInFlight++;
                Reset();
            }

            return Task.CompletedTask;
        }

        Task IReceiveObserver.PreReceive(ReceiveContext context)
        {
            lock (mutex)
            {
                receiversInFlight++;
                Reset();
            }

            return Task.CompletedTask;
        }

        Task IReceiveObserver.PostReceive(ReceiveContext context)
        {
            lock (mutex)
            {
                receiversInFlight--;
                CheckCondition();
            }

            return Task.CompletedTask;
        }

        Task IReceiveObserver.PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
        {
            return Task.CompletedTask;
        }

        Task IReceiveObserver.ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
        {
            return Task.CompletedTask;
        }

        Task IReceiveObserver.ReceiveFault(ReceiveContext context, Exception exception)
        {
            lock (mutex)
            {
                receiversInFlight--;
                CheckCondition();
            }

            return Task.CompletedTask;
        }

        void IDisposable.Dispose() => timer.Dispose();

        private void Reset()
        {
            inactive.Reset();
            timer.Stop();
        }

        private void CheckCondition()
        {
            if (consumersInFlight == 0 && receiversInFlight == 0)
            {
                timer.Restart();
            }
        }

        private void OnTimer(object? timer)
        {
            lock (mutex)
            {
                if (consumersInFlight == 0 && receiversInFlight == 0)
                {
                    inactive.Set();
                }
            }
        }
    }
}
