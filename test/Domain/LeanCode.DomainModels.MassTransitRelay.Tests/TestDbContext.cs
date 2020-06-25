using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public class TestDbContext : DbContext, IConsumedMessagesContext, IOutboxContext
    {
        public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();
        public DbSet<RaisedEvent> RaisedEvents => Set<RaisedEvent>();

        public TestDbContext()
            : base(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options)
        { }

        public TestDbContext(DbConnection connection)
            : base(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConsumedMessage.Configure(modelBuilder);
            RaisedEvent.Configure(modelBuilder);
        }

        Task IConsumedMessagesContext.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);
        Task IOutboxContext.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);
        public DbContext Self => this;
    }
}
