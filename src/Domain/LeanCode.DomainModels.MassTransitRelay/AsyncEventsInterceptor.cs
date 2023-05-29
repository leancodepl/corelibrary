using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MassTransitRelay;

[SuppressMessage("?", "CA1822", Justification = "Forcing these to be instance methods makes for better design.")]
public sealed class AsyncEventsInterceptor
{
    private static readonly EventInterceptor Interceptor = new EventInterceptor();

    public void Configure() => DomainEvents.SetInterceptor(Interceptor);

    public void Prepare() => Interceptor.Storage.Value = new ConcurrentQueue<IDomainEvent>();

    public ConcurrentQueue<IDomainEvent>? CaptureQueue()
    {
        var result = Interceptor.Storage.Value;
        Interceptor.Storage.Value = null;
        return result;
    }

    public ConcurrentQueue<IDomainEvent>? PeekQueue() => Interceptor.Storage.Value;

    private sealed class EventInterceptor : IDomainEventInterceptor
    {
        public AsyncLocal<ConcurrentQueue<IDomainEvent>?> Storage { get; } = new();

        void IDomainEventInterceptor.Intercept(IDomainEvent domainEvent)
        {
            if (Storage.Value is null)
            {
                throw new InvalidOperationException(
                    "Use IEventsExecutor or RequestEventsExecutor middleware to handle per-async requests."
                );
            }

            Storage.Value.Enqueue(domainEvent);
        }
    }
}
