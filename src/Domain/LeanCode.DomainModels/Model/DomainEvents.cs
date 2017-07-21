namespace LeanCode.DomainModels.Model
{
    public static class DomainEvents
    {
        private static IDomainEventInterceptor eventInterceptor;

        public static void SetInterceptor(IDomainEventInterceptor interceptor)
        {
            eventInterceptor = interceptor;
        }

        public static void Raise<TEvent>(TEvent domainEvent)
            where TEvent : IDomainEvent
        {
            eventInterceptor?.Intercept(domainEvent);
        }
    }
}
