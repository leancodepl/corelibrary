using Autofac;
using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using LeanCode.Components;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public class PersistedGrantStoreModule : AppModule
    {
        public override Profile MapperProfile { get; } = new PersistedGrantStoreProfile();

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PersistedGrantStore>()
                .As<IPersistedGrantStore>();
        }
    }
}
