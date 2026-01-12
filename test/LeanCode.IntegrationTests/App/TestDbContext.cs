using LeanCode.DomainModels.EF;
using LeanCode.Firebase.FCM;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App;

public class TestDbContext : DbContext
{
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<Meeting> Meetings => Set<Meeting>();

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

        modelBuilder.ConfigurePushNotificationTokenEntity<PNUserId>(setTokenColumnMaxLength: true);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<EntityId>().ArePrefixedTypedId();
        configurationBuilder.Properties<MeetingId>().ArePrefixedTypedId();
        configurationBuilder.Properties<PNUserId>().AreGuidTypedId();
    }
}
