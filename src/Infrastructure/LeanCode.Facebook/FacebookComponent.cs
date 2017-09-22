using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Facebook
{
    /// <summary>
    /// Registers <see cref="FacebookClient" /> along with the configuration.
    /// If needed, use <see cref="FacebookClient" /> directly.
    /// </summary>
    public class FacebookComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }

        private FacebookComponent(IConfiguration config)
        {
            AutofacModule = new FacebookModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static FacebookComponent WithoutConfiguration() => new FacebookComponent(null);
        public static FacebookComponent WithConfiguration(IConfiguration config) => new FacebookComponent(config);
    }
}
