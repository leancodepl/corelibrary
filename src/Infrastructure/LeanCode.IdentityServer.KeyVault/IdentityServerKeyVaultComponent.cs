using Autofac;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.IdentityServer.KeyVault
{
    public class IdentityServerKeyVaultComponent : IAppComponent
    {
        public IModule AutofacModule { get; }

        public Profile MapperProfile { get; }

        private IdentityServerKeyVaultComponent (IConfiguration config)
        {
            AutofacModule = new IdentityServerKeyVaultModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static IdentityServerKeyVaultComponent WithoutConfiguration()
            => new IdentityServerKeyVaultComponent(null);
        public static IdentityServerKeyVaultComponent WithConfiguration(IConfiguration config)
            => new IdentityServerKeyVaultComponent(config);
    }
}
