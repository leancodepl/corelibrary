using System;
using LeanCode.Components;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;

namespace LeanCode.DomainModels.MassTransitRelay.AzureServiceBus
{
    public class MassTransitServiceBusRelayModule : MassTransitRelayModuleBase
    {
        private readonly string connectionString;

        public MassTransitServiceBusRelayModule(
            string connectionString,
            string queueName,
            TypesCatalog consumers,
            BusConfig? busConfig = null)
            : base(queueName, consumers, busConfig)
        {
            this.connectionString = connectionString;
        }

        protected override IBusControl CreateBus(Action<IBusFactoryConfigurator> configurator)
        {
            return Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                cfg.Host(connectionString, h =>
                {
                    h.OperationTimeout = TimeSpan.FromSeconds(5);
                });

                configurator(cfg);
            });
        }
    }
}
