using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.MvcEventsExecutor
{
    public class MvcEventsExecutorApp : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; } = new MvcEventsExecutorModule();

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
