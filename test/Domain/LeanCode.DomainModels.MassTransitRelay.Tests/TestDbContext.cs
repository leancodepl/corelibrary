using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class TestDbContext : DbContext, IConsumedMessagesContext, IOutboxContext
{
    public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();
    public DbSet<RaisedEvent> RaisedEvents => Set<RaisedEvent>();

    public TestDbContext(DbContextOptions<TestDbContext> opts)
        : base(opts)
    { }

    public static TestDbContext Create()
    {
        return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options);
    }

    public static TestDbContext Create(DbConnection connection)
    {
        return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConsumedMessage.Configure(modelBuilder);
        RaisedEvent.Configure(modelBuilder);
    }

    public DbContext Self => this;
}
