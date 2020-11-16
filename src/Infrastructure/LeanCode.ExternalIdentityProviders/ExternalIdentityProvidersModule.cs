using System;
using Autofac;
using LeanCode.Components;
using LeanCode.ExternalIdentityProviders.Apple;
using LeanCode.ExternalIdentityProviders.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ExternalIdentityProviders
{
    public class ExternalIdentityProvidersModule<TUser> : AppModule
        where TUser : IdentityUser<Guid>
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppleExternalLogin<TUser>>().AsSelf();
            builder.RegisterType<AppleGrantValidator<TUser>>().AsImplementedInterfaces();

            builder.RegisterType<GoogleAuthService>().AsSelf();
            builder.RegisterType<GoogleExternalLogin<TUser>>().AsSelf();
            builder.RegisterType<GoogleGrantValidator<TUser>>().AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(ExternalLoginExceptionHandler<>)).AsSelf();
        }

        public override void ConfigureServices(IServiceCollection services) =>
            services.AddHttpClient<AppleIdService>();
    }
}
