using Autofac;
using IdentityServer4.Stores;
using LeanCode.Components;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public class PersistedGrantStoreModule : AppModule
    {
        protected override void Load(ContainerBuilder builder) =>
            builder.RegisterType<PersistedGrantStore>().As<IPersistedGrantStore>();
    }
}
