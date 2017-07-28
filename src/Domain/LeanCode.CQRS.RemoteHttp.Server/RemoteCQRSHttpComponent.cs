using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using LeanCode.CQRS.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    public sealed class RemoteCQRSHttpComponent<TAppContext> : IAppComponent
    {
        public IModule AutofacModule { get; }

        public Profile MapperProfile => null;

        public RemoteCQRSHttpComponent(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            AutofacModule = new RemoteCQRSHttpModule<TAppContext>(catalog, contextTranslator);
        }

        public void ConfigureServices(IServiceCollection services)
        { }
    }

    public static class RemoteCQRSHttpComponent
    {
        public static RemoteCQRSHttpComponent<TAppContext> Create<TAppContext>(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            return new RemoteCQRSHttpComponent<TAppContext>(catalog, contextTranslator);
        }
    }
}
