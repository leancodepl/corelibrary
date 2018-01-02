using Autofac;
using LeanCode.Components;

namespace LeanCode.IdentityServer.KeyVault
{
    public class IdentityServerKeyVaultModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SigningService>().AsSelf().SingleInstance();
            builder.RegisterType<KeyMaterialService>().AsImplementedInterfaces();
            builder.RegisterType<TokenCreationService>().AsImplementedInterfaces();
        }
    }
}
