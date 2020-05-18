using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Inbox
{
    public interface IConsumedMessagesContext
    {
        DbSet<ConsumedMessage> ConsumedMessages { get; }
        DbContext Self { get; }
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
