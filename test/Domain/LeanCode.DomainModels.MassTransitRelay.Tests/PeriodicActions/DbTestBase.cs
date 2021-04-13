using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests.PeriodicActions
{
    public abstract class DbTestBase : IAsyncLifetime, IDisposable
    {
        protected TestDbContext DbContext { get; }
        private readonly DbConnection dbConnection;

        protected DbTestBase()
        {
            dbConnection = new SqliteConnection("Filename=:memory:");
            DbContext = TestDbContext.Create(dbConnection);
        }

        public async Task InitializeAsync()
        {
            // harness started at test level because
            // we need to have different consumers in each test
            // await harness.Start();

            await dbConnection.OpenAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await dbConnection.CloseAsync();
            await dbConnection.DisposeAsync();
        }

        public void Dispose()
        {
        }
    }
}
