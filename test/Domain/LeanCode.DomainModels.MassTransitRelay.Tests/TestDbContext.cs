using System.Data.Common;
using System.Threading.Tasks;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    public class TestDbContext : DbContext, IConsumedMessagesContext
    {
        public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();

        public TestDbContext()
            : base(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options)
        { }

        public TestDbContext(DbConnection connection)
            : base(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConsumedMessage.Configure(modelBuilder);
        }

        public DbContext Self => this;
        Task IConsumedMessagesContext.SaveChangesAsync() => SaveChangesAsync();
    }
}
