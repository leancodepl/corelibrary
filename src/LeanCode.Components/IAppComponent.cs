using Autofac.Core;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Components
{
    public interface IAppComponent
    {
        IModule AutofacModule { get; }
        Profile MapperProfile { get; }
        void ConfigureServices(IServiceCollection services);
    }
}
