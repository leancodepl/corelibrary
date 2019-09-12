using Autofac;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Cache.AspNet
{
    public class InMemoryCacheModule : AppModule
    {
        public override void ConfigureServices(IServiceCollection services) =>
            services.AddMemoryCache();

        protected override void Load(ContainerBuilder builder) =>
            builder.RegisterType<InMemoryCacheAdapter>().As<ICacher>();
    }
}
