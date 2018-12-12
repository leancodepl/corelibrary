using System;
using System.Collections.Concurrent;
using System.Threading;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution
{
    public sealed class AsyncEventsInterceptor
    {
        private static readonly Interceptor interceptor = new Interceptor();

        public void Configure()
        {
            DomainEvents.SetInterceptor(interceptor);
        }

        public void Prepare()
        {
            interceptor.storage.Value = new ConcurrentQueue<IDomainEvent>();
        }

        public ConcurrentQueue<IDomainEvent> CaptureQueue()
        {
            var result = interceptor.storage.Value;
            interceptor.storage.Value = null;
            return result;
        }

        public ConcurrentQueue<IDomainEvent> PeekQueue() => interceptor.storage.Value;

        private sealed class Interceptor
            : IDomainEventInterceptor
        {
            public readonly AsyncLocal<ConcurrentQueue<IDomainEvent>> storage
                = new AsyncLocal<ConcurrentQueue<IDomainEvent>>();

            void IDomainEventInterceptor.Intercept(IDomainEvent domainEvent)
            {
                if (storage.Value is null)
                {
                    throw new InvalidOperationException(
                        "Use IEventsExecutor or RequestEventsExecutor middleware to handle per-async requests.");
                }

                storage.Value.Enqueue(domainEvent);
            }
        }
    }
}
