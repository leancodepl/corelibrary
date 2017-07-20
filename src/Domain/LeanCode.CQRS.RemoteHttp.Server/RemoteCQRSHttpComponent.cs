using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSHttpComponent : IAppComponent
    {
        public IModule AutofacModule { get; }

        public Profile MapperProfile => null;

        public RemoteCQRSHttpComponent(TypesCatalog catalog)
        {
            AutofacModule = new RemoteHttpModule(catalog);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
