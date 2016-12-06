using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Cache.AspNet
{
    public class InMemoryCacheApp : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; } = new InMemoryCacheModule();

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
