using Autofac;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;

namespace LeanCode.IdentityServer.KeyVault
{
    class IdentityServerKeyVaultModule : Module
    {
        private readonly IConfiguration config;

        public IdentityServerKeyVaultModule(IConfiguration config)
        {
            this.config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (config != null)
            {
                builder.ConfigSection<IdentityServerKeyVaultConfiguration>(config);
            }
        }
    }
}
