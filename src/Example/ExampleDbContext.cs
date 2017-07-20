using LeanCode.PushNotifications.EF;
using Microsoft.EntityFrameworkCore;

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
}
