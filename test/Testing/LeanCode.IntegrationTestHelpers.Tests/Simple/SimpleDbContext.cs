using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTestHelpers.Tests.Simple
{
    public class SimpleDbContext : DbContext
    {
        public DbSet<Entity> Entities => Set<Entity>();

        public SimpleDbContext(DbContextOptions<SimpleDbContext> opts)
            : base(opts)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>(cfg =>
            {
                cfg.HasKey(e => e.Id);
                cfg.Property(e => e.Value).HasMaxLength(10);
            });
        }
    }
}
