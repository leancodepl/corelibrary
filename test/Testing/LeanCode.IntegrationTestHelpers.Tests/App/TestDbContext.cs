using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class Entity
    {
        public int Id { get; set; }
        public string? Data { get; set; }
    }

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
                cfg.Property(e => e.Id).ValueGeneratedNever();
                cfg.Property(e => e.Data);
            });
        }
    }
}
