using System;
using Autofac;
using LeanCode.Components;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.RemoteHttp.Server
{
    class RemoteCQRSHttpModule<TAppContext> : Autofac.Module
    {
        private readonly TypesCatalog catalog;
        private readonly Func<HttpContext, TAppContext> contextTranslator;

        public RemoteCQRSHttpModule(
            TypesCatalog catalog,
            Func<HttpContext, TAppContext> contextTranslator)
        {
            this.catalog = catalog;
            this.contextTranslator = contextTranslator;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RemoteCommandHandler<TAppContext>>()
                .AsImplementedInterfaces()
                .SingleInstance()
                .WithParameter(nameof(catalog), catalog)
                .WithParameter(nameof(contextTranslator), contextTranslator);

            builder.RegisterType<RemoteQueryHandler<TAppContext>>()
                .AsImplementedInterfaces()
                .SingleInstance()
                .WithParameter(nameof(catalog), catalog)
                .WithParameter(nameof(contextTranslator), contextTranslator);
        }
    }
}
