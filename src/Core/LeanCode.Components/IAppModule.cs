using Autofac;
using Autofac.Core;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Components
{
    public interface IAppModule : IModule
    {
        Profile MapperProfile { get; }
        void ConfigureServices(IServiceCollection services);
    }

    public class AppModule : Module, IAppModule
    {
        public virtual Profile MapperProfile { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        { }
    }
}
