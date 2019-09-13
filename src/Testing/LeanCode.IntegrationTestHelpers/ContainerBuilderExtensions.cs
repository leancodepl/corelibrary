using Autofac;
using Autofac.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeanCode.IntegrationTestHelpers
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<TContext, ConcreteReflectionActivatorData, SingleRegistrationStyle> AddTestDbContext<TContext>(
            this ContainerBuilder builder, IntegrationTestContextBase testBase)
            where TContext : notnull, DbContext
        {
            return builder.RegisterType<TContext>()
                .AsSelf()
                .As<DbContext>()
                .WithParameter(
                    (pi, _) => pi.ParameterType == typeof(DbContextOptions<TContext>),
                    (_, cc) => PrepareDbOptions<TContext>(cc, testBase.ConnectionString))
                .InstancePerLifetimeScope();
        }

        private static DbContextOptions<TContext> PrepareDbOptions<TContext>(IComponentContext context, string connStr)
            where TContext : notnull, DbContext
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseLoggerFactory(context.Resolve<ILoggerFactory>())
                .UseSqlServer(connStr)
                .Options;
        }
    }
}
