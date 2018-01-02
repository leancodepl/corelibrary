using Autofac;
using LeanCode.Components;

namespace LeanCode.Cache.AspNet
{
    public class InMemoryCacheModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryCacheAdapter>().As<ICacher>();
        }
    }
}
