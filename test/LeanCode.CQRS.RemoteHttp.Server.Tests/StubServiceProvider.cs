using System;
using Autofac.Features.Indexed;
using LeanCode.Components;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    class StubServiceProvider : IServiceProvider
    {
        private readonly RemoteCommandHandler command;
        private readonly RemoteQueryHandler query;

        public StubServiceProvider(RemoteCommandHandler command, RemoteQueryHandler query)
        {
            this.command = command;
            this.query = query;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IIndex<TypesCatalog, RemoteCommandHandler>))
            {
                return StubIndex.Create(command);
            }
            else if (serviceType == typeof(IIndex<TypesCatalog, RemoteQueryHandler>))
            {
                return StubIndex.Create(query);
            }
            throw new NotImplementedException();
        }
    }
}
