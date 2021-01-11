using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using LeanCode.Firebase.FCM.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.Tests.EntityFramework
{
    public class TestDbContext : DbContext
    {
        private readonly DbConnection connection;

        public DbSet<PushNotificationTokenEntity> Tokens { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
            connection = Database.GetDbConnection();
        }

        public static async Task<TestDbContext> CreateInMemory()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options);
            await context.connection.OpenAsync();
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PushNotificationTokenEntity.Configure(modelBuilder);
        }

        public override async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
            await base.DisposeAsync();
        }

        // Required to make tests work in SQLite that assumes `Guid == string`
        private class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
        {
            public override void SetValue(IDbDataParameter parameter, Guid guid) => parameter.Value = guid.ToString();
            public override Guid Parse(object value) => Guid.Parse((string)value);
        }
    }
}
