using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App;

public class TestDbContext : DbContext
{
    public DbSet<Entity> Entities => Set<Entity>();

    public TestDbContext(DbContextOptions<TestDbContext> opts)
        : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>(cfg =>
        {
            cfg.HasKey(e => e.Id);
            cfg.Property(e => e.Value).HasMaxLength(10);
        });
    }
}
