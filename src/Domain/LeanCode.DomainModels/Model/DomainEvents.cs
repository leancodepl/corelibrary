namespace LeanCode.DomainModels.Model;

public static class DomainEvents
{
    public static IDomainEventInterceptor? EventInterceptor { get; private set; }

    public static void SetInterceptor(IDomainEventInterceptor interceptor)
    {
        EventInterceptor = interceptor;
    }

    public static void Raise<TEvent>(TEvent domainEvent)
        where TEvent : class, IDomainEvent
    {
        EventInterceptor?.Intercept(domainEvent);
    }
}
