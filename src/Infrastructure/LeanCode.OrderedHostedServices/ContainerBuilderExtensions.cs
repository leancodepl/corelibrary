using System;
using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.Hosting;

namespace LeanCode.OrderedHostedServices
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            AddOrderedHostedService<T>(this ContainerBuilder builder)
            where T : IOrderedHostedService
        {
            return builder.RegisterType<T>()
                .As<IOrderedHostedService>();
        }

        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            AddOrderedHostedService<T>(this ContainerBuilder builder, int order)
            where T : IHostedService
        {
            if (typeof(T).IsAssignableTo<IOrderedHostedService>())
            {
                throw new ArgumentException("Cannot register `IOrderedHostedService` as a wrapped `IHostedService`.");
            }

            builder.RegisterType<HostedServiceWrapper<T>>()
                .WithParameter("order", order)
                .As<IOrderedHostedService>();
            return builder.RegisterType<T>();
        }

        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            AddHostedService<T>(this ContainerBuilder builder)
            where T : IHostedService
        {
            if (typeof(T).IsAssignableTo<IOrderedHostedService>())
            {
                throw new ArgumentException("Cannot register `IOrderedHostedService` as a normal `IHostedService`.");
            }

            return builder.RegisterType<T>()
                .As<IHostedService>();
        }
    }
}
