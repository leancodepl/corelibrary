using Autofac;
using Autofac.Extensions.DependencyInjection;
using IdentityServer4.Validation;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.ExternalIdentityProviders.Tests
{
    public class ExternalIdentityProvidersModuleTests
    {
        [Fact]
        public void Facebook_is_registered()
        {
            var container = Prepare(Providers.Facebook);

            Assert.True(container.IsRegistered<FacebookClient>());
            Assert.True(container.IsRegistered<FacebookExternalLogin<User>>());
            Assert.True(container.IsRegistered<FacebookGrantValidator<User>>());
            Assert.True(container.IsRegistered<IExtensionGrantValidator>());
        }

        [Fact]
        public void Apple_is_registered()
        {
            var container = Prepare(Providers.Apple);

            Assert.True(container.IsRegistered<AppleIdService>());
            Assert.True(container.IsRegistered<AppleExternalLogin<User>>());
            Assert.True(container.IsRegistered<AppleGrantValidator<User>>());
            Assert.True(container.IsRegistered<IExtensionGrantValidator>());
        }

        [Fact]
        public void Google_is_registered()
        {
            var container = Prepare(Providers.Google);

            Assert.True(container.IsRegistered<GoogleAuthService>());
            Assert.True(container.IsRegistered<GoogleExternalLogin<User>>());
            Assert.True(container.IsRegistered<GoogleGrantValidator<User>>());
            Assert.True(container.IsRegistered<IExtensionGrantValidator>());
        }

        [Fact]
        public void Registers_only_selected_providers()
        {
            var container = Prepare(Providers.Apple | Providers.Google);

            Assert.False(container.IsRegistered<FacebookExternalLogin<User>>());
            Assert.True(container.IsRegistered<AppleExternalLogin<User>>());
            Assert.True(container.IsRegistered<GoogleExternalLogin<User>>());
            Assert.True(container.IsRegistered<IExtensionGrantValidator>());
        }

        private static IContainer Prepare(Providers providers)
        {
            var builder = new ContainerBuilder();
            var services = new ServiceCollection();
            var module = new ExternalIdentityProvidersModule<User>(providers);

            builder.RegisterModule(module);
            module.ConfigureServices(services);
            builder.Populate(services);

            return builder.Build();
        }
    }
}
