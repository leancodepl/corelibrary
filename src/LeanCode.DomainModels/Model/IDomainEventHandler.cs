using System.Threading.Tasks;

namespace LeanCode.DomainModels.Model
{
    public interface IDomainEventHandler<in TEvent>
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent);
    }
}
