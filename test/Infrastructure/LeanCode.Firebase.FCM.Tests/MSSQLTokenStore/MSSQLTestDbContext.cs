using System.Data;
using System.Data.Common;
using Dapper;
using LeanCode.Firebase.FCM.MSSQL;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.Tests.MSSQLTokenStore
{
    public class MSSQLTestDbContext : DbContext
    {
        private readonly DbConnection connection;

        public DbSet<MSSQLPushNotificationTokenEntity> Tokens { get; set; }

        public MSSQLTestDbContext(DbContextOptions<MSSQLTestDbContext> options)
            : base(options)
        {
            connection = Database.GetDbConnection();
        }

        public static async Task<MSSQLTestDbContext> CreateInMemory()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            var context = new MSSQLTestDbContext(new DbContextOptionsBuilder<MSSQLTestDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options);
            await context.connection.OpenAsync();
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MSSQLPushNotificationTokenEntity.Configure(modelBuilder);
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
