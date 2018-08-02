using Autofac;

namespace LeanCode.IntegrationTestHelpers.Tests.Simple
{
    public class SimpleContext : IntegrationTestContextBase
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddTestDbContext<SimpleDbContext>(this);
        }
    }
}
