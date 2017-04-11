namespace LeanCode.DomainModels.Model
{
    public static class DomainEvents
    {
        private static IDomainEventStorage eventsStorage;

        public static void SetStorage(IDomainEventStorage storage)
        {
            eventsStorage = storage;
        }

        public static void Raise<TEvent>(TEvent domainEvent)
            where TEvent : IDomainEvent
        {
            eventsStorage?.Store(domainEvent);
        }
    }
}
