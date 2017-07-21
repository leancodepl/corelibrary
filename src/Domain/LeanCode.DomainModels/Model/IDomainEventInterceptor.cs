namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventInterceptor
    {
        void Intercept(IDomainEvent domainEvent);
    }
}
