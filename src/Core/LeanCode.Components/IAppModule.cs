using Autofac;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Components
{
    public interface IAppModule : IModule
    {
        void ConfigureServices(IServiceCollection services);
    }

    public class AppModule : Module, IAppModule
    {
        public virtual void ConfigureServices(IServiceCollection services)
        { }
    }
}
