using Autofac;
using LeanCode.Components;
using Microsoft.AspNetCore.Hosting.Server;

namespace LeanCode.AsyncInitializer
{
    public class AsyncInitializerModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AsyncInitializer>().SingleInstance();
            builder.RegisterDecorator<AsyncInitializerServer, IServer>();
        }
    }
}
