using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.IntegrationTestHelpers.Tests.CQRS
{
    public class CQRSTestsContext : CQRSTestContextBase<AppContext>
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddTestDbContext<TestDbContext>(this);
        }
    }

    public class CQRSTests : CQRSTestBase<CQRSTestsContext, AppContext>
    {
        public CQRSTests(CQRSTestsContext context)
            : base(context)
        { }

        [TestStep]
        public async Task Step01_Run_query()
        {
            var res = await GetAsync(new TestQuery());

            Assert.Equal("abc", res.Value);
        }

        [TestStep]
        public async Task Step02_Execute_command()
        {
            await RunAsync<EmptyContext, TestCommand>(new TestCommand { Name = "test" });

            var entity = await Context.With<TestDbContext, Entity>(
                d => d.Entities.SingleOrDefaultAsync());
            Assert.NotNull(entity);
            Assert.Equal("test", entity.Value);
        }

        protected override AppContext GetDefaultContext() => new AppContext();
    }
}
