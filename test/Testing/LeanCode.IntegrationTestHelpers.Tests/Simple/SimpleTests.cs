using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests.Simple
{
    public class SimpleTests : IClassFixture<SimpleContext>
    {
        private readonly SimpleContext ctx;

        public SimpleTests(SimpleContext ctx)
        {
            this.ctx = ctx;
        }

        [PreparationStep]
        public Task Step01_Insert_some_data()
        {
            return ctx.With<TestDbContext>(c =>
            {
                c.Entities.Add(new Entity { Id = Guid.NewGuid(), Value = "abc" });
                return c.SaveChangesAsync();
            });
        }

        [TestStep]
        public async Task Step02_Assert_the_data()
        {
            var data = await ctx.With<TestDbContext, List<Entity>>(d => d.Entities.ToListAsync());

            var e = Assert.Single(data);
            Assert.Equal("abc", e.Value);
        }
    }
}
