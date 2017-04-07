using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.RequestEventsExecutor
{
    public class RequestEventsExecutorComponent : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; } = new RequestEventsExecutorModule();

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
