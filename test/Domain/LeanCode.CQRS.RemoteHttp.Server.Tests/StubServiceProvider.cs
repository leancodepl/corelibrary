using System;

namespace LeanCode.CQRS.RemoteHttp.Server.Tests
{
    class StubServiceProvider : IServiceProvider
    {
        private readonly RemoteCommandHandler<AppContext> command;
        private readonly RemoteQueryHandler<AppContext> query;

        public StubServiceProvider(RemoteCommandHandler<AppContext> command, RemoteQueryHandler<AppContext> query)
        {
            this.command = command;
            this.query = query;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IRemoteCommandHandler))
            {
                return command;
            }
            else if (serviceType == typeof(IRemoteQueryHandler))
            {
                return query;
            }
            throw new NotImplementedException();
        }
    }
}
