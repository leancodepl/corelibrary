using System;
using Autofac;
using LeanCode.Components;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;

namespace LeanCode.DomainModels.MassTransitRelay.AzureServiceBus
{
    public class MassTransitServiceBusRelayModule : MassTransitRelayModuleBase
    {
        private readonly string connectionString;
        private readonly string queueName;

        public MassTransitServiceBusRelayModule(
            string connectionString,
            string queueName,
            TypesCatalog consumers)
            : base(consumers)
        {
            this.connectionString = connectionString;
            this.queueName = queueName;
        }

        protected override IBusControl CreateBus(IComponentContext context)
        {
            return Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                cfg.Host(connectionString, h =>
                {
                    h.OperationTimeout = TimeSpan.FromSeconds(5);
                });

                ConfigureCommonFilters(cfg, context);

                cfg.ReceiveEndpoint(queueName, e =>
                {
                    e.ConfigureConsumers(context);
                });
            });
        }
    }
}
