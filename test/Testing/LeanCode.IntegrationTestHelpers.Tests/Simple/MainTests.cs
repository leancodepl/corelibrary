using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests.Simple
{
    public class MainTests : IClassFixture<SimpleContext>
    {
        private readonly SimpleContext ctx;

        public MainTests(SimpleContext ctx)
        {
            this.ctx = ctx;
        }

        [PreparationStep]
        public Task Insert_some_data()
        {
            return WithDb(c =>
            {
                c.Entities.Add(new Entity { Id = Guid.NewGuid(), Value = "abc" });
                return c.SaveChangesAsync();
            });
        }

        [TestStep]
        public async Task Assert_the_data()
        {
            var data = await WithDb(d => d.Entities.ToListAsync());

            var e = Assert.Single(data);
            Assert.Equal("abc", e.Value);
        }

        private async Task<T> WithDb<T>(Func<SimpleDbContext, Task<T>> exec)
        {
            using (var scope = ctx.Container.BeginLifetimeScope())
            using (var db = scope.Resolve<SimpleDbContext>())
            {
                return await exec(db);
            }
        }
    }
}
