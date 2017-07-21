using System;
using System.Collections.Concurrent;
using System.Threading;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution
{
    public sealed class AsyncEventsInterceptor
        : IDomainEventInterceptor
    {
        private readonly AsyncLocal<ConcurrentQueue<IDomainEvent>> storage
            = new AsyncLocal<ConcurrentQueue<IDomainEvent>>();

        void IDomainEventInterceptor.Intercept(IDomainEvent domainEvent)
        {
            if (storage.Value == null)
            {
                throw new InvalidOperationException(
                    "Use IEventsExecutor or RequestEventsExecutor middleware to handle per-async requests.");
            }

            storage.Value.Enqueue(domainEvent);
        }

        public void Prepare()
        {
            storage.Value = new ConcurrentQueue<IDomainEvent>();
        }

        public ConcurrentQueue<IDomainEvent> CaptureQueue()
        {
            var result = storage.Value;
            storage.Value = null;
            return result;
        }

        public ConcurrentQueue<IDomainEvent> PeekQueue() => storage.Value;
    }
}
