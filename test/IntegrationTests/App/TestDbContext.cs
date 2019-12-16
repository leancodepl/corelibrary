using LeanCode.Firebase.FCM.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IntegrationTests.App
{
    public class TestDbContext : DbContext
    {
        public DbSet<PushNotificationTokenEntity> PNTokens => Set<PushNotificationTokenEntity>();

        public TestDbContext(DbContextOptions<TestDbContext> opts)
            : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PushNotificationTokenEntity.Configure(modelBuilder);
        }
    }
}
