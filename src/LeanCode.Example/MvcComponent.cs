using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Example
{
    public class MvcComponent : IAppComponent
    {
        public IModule AutofacModule => null;
        public Profile MapperProfile => null;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }
    }
}
