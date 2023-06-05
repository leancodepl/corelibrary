using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

public class TestDbContext : DbContext
{
    public DbSet<HandledLog> HandledLog => Set<HandledLog>();

    public TestDbContext(DbContextOptions<TestDbContext> opts)
        : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HandledLog>(cfg => cfg.HasKey(e => new { e.CorrelationId, e.HandlerName }));

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
