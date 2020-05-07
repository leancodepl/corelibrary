using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Outbox
{
    public interface IOutboxContext
    {
        DbSet<RaisedEvent> RaisedEvents { get; }
        DbContext Self { get; }
        Task SaveChangesAsync();
    }
}
