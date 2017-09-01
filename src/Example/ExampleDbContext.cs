using LeanCode.PushNotifications.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeanCode.Example
{
    public class ExampleDbContext : DbContext
    {
        public DbSet<PushNotificationTokenEntity> Tokens { get; set; }

        public ExampleDbContext(DbContextOptions<ExampleDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PushNotificationTokenEntity.Configure(modelBuilder);
        }
    }

    public class ExampleDbContextDesignFactory : IDesignTimeDbContextFactory<ExampleDbContext>
    {
        public ExampleDbContext CreateDbContext(string[] args)
        {
            var optsBuilder = new DbContextOptionsBuilder<ExampleDbContext>()
                .UseSqlite("Data Source=example.db");
            return new ExampleDbContext(optsBuilder.Options);
        }
    }
}
