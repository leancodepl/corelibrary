using Autofac;

namespace LeanCode.Cache.AspNet
{
    class InMemoryCacheModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryCacheAdapter>().As<ICacher>();
        }
    }
}
