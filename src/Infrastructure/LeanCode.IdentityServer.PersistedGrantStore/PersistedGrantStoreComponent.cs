using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    public class PersistedGrantStoreComponent : IAppComponent
    {
        public IModule AutofacModule { get; } = new PersistedGrantStoreModule();
        public Profile MapperProfile { get; } = new PersistedGrantStoreProfile();

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
