using System.Collections.Concurrent;
using System.Threading;
using Autofac;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.MvcEventsExecutor
{
    public class PerRequestEventsStorage : IDomainEventStorage, IStartable
    {
        private readonly AsyncLocal<ConcurrentQueue<IDomainEvent>> storage
            = new AsyncLocal<ConcurrentQueue<IDomainEvent>>();

        public void UseNewQueue()
        {
            storage.Value = new ConcurrentQueue<IDomainEvent>();
        }

        public ConcurrentQueue<IDomainEvent> CaptureQueue()
        {
            var result = storage.Value;
            storage.Value = null;
            return result;
        }

        public void Store(IDomainEvent domainEvent)
        {
            storage.Value.Enqueue(domainEvent);
        }

        public void Start()
        {
            DomainEvents.SetStorage(this);
        }

        public ConcurrentQueue<IDomainEvent> PeekQueue()
        {
            return storage.Value;
        }
    }
}
