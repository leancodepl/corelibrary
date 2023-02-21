using MassTransit;
using MassTransit.Testing.Implementations;
using MassTransit.Util;

namespace LeanCode.DomainModels.MassTransitRelay.Testing;

#pragma warning disable 0420
public sealed class ResettableBusActivityMonitor :
    IBusActivityMonitor,
    IReceiveObserver,
    IConsumeObserver,
    ISendObserver,
    IPublishObserver,
    IDisposable
{
    private readonly object mutex = new object();
    private readonly AsyncManualResetEvent inactive = new(true);
    private readonly RollingTimer timer;

    private volatile int consumersInFlight;
    private volatile int receiversInFlight;
    private volatile int sendInFlight;
    private volatile int publishInFlight;

    private bool HasStabilized => consumersInFlight == 0 && receiversInFlight == 0 && sendInFlight == 0 && publishInFlight == 0;

    public ResettableBusActivityMonitor(TimeSpan inactivityWaitTime)
    {
        timer = new RollingTimer(OnTimer, inactivityWaitTime);
    }

    public static ResettableBusActivityMonitor CreateFor(IBusControl bus, TimeSpan inactivityWaitTime)
    {
        var monitor = new ResettableBusActivityMonitor(inactivityWaitTime);
        bus.ConnectConsumeObserver(monitor);
        bus.ConnectReceiveObserver(monitor);
        bus.ConnectSendObserver(monitor);
        bus.ConnectPublishObserver(monitor);
        return monitor;
    }

    public Task AwaitBusInactivity() => inactive.WaitAsync();
    public Task<bool> AwaitBusInactivity(TimeSpan timeout) => inactive.WaitAsync(timeout);
    public Task AwaitBusInactivity(CancellationToken cancellationToken) => inactive.WaitAsync(cancellationToken);

    Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception) => Decrement(ref consumersInFlight);
    Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context) => Decrement(ref consumersInFlight);
    Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context) => Increment(ref consumersInFlight);
    Task IReceiveObserver.PreReceive(ReceiveContext context) => Increment(ref receiversInFlight);
    Task IReceiveObserver.PostReceive(ReceiveContext context) => Decrement(ref receiversInFlight);
    Task IReceiveObserver.ReceiveFault(ReceiveContext context, Exception exception) => Decrement(ref receiversInFlight);
    Task IReceiveObserver.PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) => Task.CompletedTask;
    Task IReceiveObserver.ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) => Task.CompletedTask;
    Task ISendObserver.PreSend<T>(SendContext<T> context) => Increment(ref sendInFlight);
    Task ISendObserver.PostSend<T>(SendContext<T> context) => Decrement(ref sendInFlight);
    Task ISendObserver.SendFault<T>(SendContext<T> context, Exception exception) => Decrement(ref sendInFlight);
    Task IPublishObserver.PrePublish<T>(PublishContext<T> context) => Increment(ref publishInFlight);
    Task IPublishObserver.PostPublish<T>(PublishContext<T> context) => Decrement(ref publishInFlight);
    Task IPublishObserver.PublishFault<T>(PublishContext<T> context, Exception exception) => Decrement(ref publishInFlight);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1063", Justification = "We want clean API.")]
    void IDisposable.Dispose() => timer.Dispose();

    private Task Increment(ref int counter)
    {
        lock (mutex)
        {
            Interlocked.Increment(ref counter);
            inactive.Reset();
            timer.Stop();
        }

        return Task.CompletedTask;
    }

    private Task Decrement(ref int counter)
    {
        lock (mutex)
        {
            Interlocked.Decrement(ref counter);

            if (HasStabilized)
            {
                timer.Restart();
            }
        }

        return Task.CompletedTask;
    }

    private void OnTimer(object? timer)
    {
        lock (mutex)
        {
            if (HasStabilized)
            {
                inactive.Set();
            }
        }
    }
}
#pragma warning restore 420

