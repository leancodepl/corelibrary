using System;
using Autofac;
using LeanCode.Components;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Facebook;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ExternalIdentityProviders
{
    [Flags]
    public enum Providers
    {
        Facebook = 0b001,
        Apple = 0b010,
        Google = 0b100,

        None = 0b000,
        All = 0b111,
    }

    public class ExternalIdentityProvidersModule<TUser> : AppModule
        where TUser : IdentityUser<Guid>
    {
        private readonly Providers configuration;

        public ExternalIdentityProvidersModule(Providers configuration)
        {
            if ((configuration & Providers.All) == Providers.None)
            {
                throw new ArgumentException("At least one identity provider is required.", nameof(configuration));
            }

            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (IsEnabled(Providers.Facebook))
            {
                builder.RegisterType<FacebookExternalLogin<TUser>>().AsSelf();
                builder.RegisterType<FacebookGrantValidator<TUser>>().AsSelf().AsImplementedInterfaces();
            }

            if (IsEnabled(Providers.Apple))
            {
                builder.RegisterType<AppleExternalLogin<TUser>>().AsSelf();
                builder.RegisterType<AppleGrantValidator<TUser>>().AsSelf().AsImplementedInterfaces();
            }

            if (IsEnabled(Providers.Google))
            {
                builder.RegisterType<GoogleAuthService>().AsSelf();
                builder.RegisterType<GoogleExternalLogin<TUser>>().AsSelf();
                builder.RegisterType<GoogleGrantValidator<TUser>>().AsSelf().AsImplementedInterfaces();
            }

            builder.RegisterGeneric(typeof(ExternalLoginExceptionHandler<>)).AsSelf();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (IsEnabled(Providers.Facebook))
            {
                services.AddHttpClient<FacebookClient>(c => c.BaseAddress = new Uri(FacebookClient.ApiBase));
            }

            if (IsEnabled(Providers.Apple))
            {
                services.AddHttpClient<AppleIdService>();
            }
        }

        private bool IsEnabled(Providers provider) => configuration.HasFlag(provider);
    }
}
