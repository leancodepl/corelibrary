using System.Data;
using System.Data.Common;
using Dapper;
using LeanCode.Firebase.FCM.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.Tests.MsSqlTokenStore;

public class SqliteTestDbContext : DbContext
{
    private readonly DbConnection connection;

    public DbSet<MsSqlPushNotificationTokenEntity> Tokens { get; set; }

    public SqliteTestDbContext(DbContextOptions<SqliteTestDbContext> options)
        : base(options)
    {
        connection = Database.GetDbConnection();
    }

    public static async Task<SqliteTestDbContext> CreateInMemory()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        var context = new SqliteTestDbContext(
            new DbContextOptionsBuilder<SqliteTestDbContext>().UseSqlite("Filename=:memory:").Options
        );
        await context.connection.OpenAsync();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MsSqlPushNotificationTokenEntity.Configure(modelBuilder);
    }

    public override async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
        await base.DisposeAsync();
    }

    // Required to make tests work in SQLite that assumes `Guid == string`
    private sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid guid) => parameter.Value = guid.ToString();

        public override Guid Parse(object value) => Guid.Parse((string)value);
    }
}
