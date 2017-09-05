using Autofac;
using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    class PersistedGrantStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PersistedGrantStore>()
                .As<IPersistedGrantStore>();
        }
    }
}
