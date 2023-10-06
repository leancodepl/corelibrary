using LeanCode.Firebase.FCM;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App;

public class TestDbContext : DbContext
{
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<PushNotificationTokenEntity<Guid>> Tokens => Set<PushNotificationTokenEntity<Guid>>();

    public TestDbContext(DbContextOptions<TestDbContext> opts)
        : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>(cfg =>
        {
            cfg.HasKey(e => e.Id);
            cfg.Property(e => e.Value).HasMaxLength(100);
        });

        modelBuilder.Entity<Meeting>(cfg =>
        {
            cfg.HasKey(e => e.Id);
            cfg.OwnsOne(e => e.StartTime);
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ConfigurePushNotificationTokenEntity<Guid>(setTokenColumnMaxLength: true);
    }
}
