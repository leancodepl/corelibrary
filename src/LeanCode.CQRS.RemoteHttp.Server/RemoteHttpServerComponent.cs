using System.Reflection;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteHttpServerComponent : IAppComponent
    {
        public IModule AutofacModule { get; }

        public Profile MapperProfile => null;

        public RemoteHttpServerComponent(Assembly typesAssembly)
        {
            AutofacModule = new RemoteHttpModule(typesAssembly);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
