using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> opts)
        : base(opts) { }

    public static TestDbContext Create()
    {
        return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseSqlite("Filename=:memory:").Options);
    }

    public static TestDbContext Create(DbConnection connection)
    {
        return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) { }

    public DbContext Self => this;
}
