using MassTransit;
using NSubstitute;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public class MassTransitRegistrationConfigurationExtensionsTests
{
    private readonly IRegistrationConfigurator configurator = Substitute.For<IRegistrationConfigurator>();

    [Fact]
    public void Registers_consumers_with_definition_from_assembly()
    {
        configurator.AddConsumersWithDefaultConfiguration(
            new[] { typeof(Consumer1).Assembly, },
            typeof(DefaultConsumerDefinition<>)
        );

        VerifyConsumerRegistration<Consumer1, DefaultConsumerDefinition<Consumer1>>();
        VerifyConsumerRegistration<Consumer2, DefaultConsumerDefinition<Consumer2>>();
        VerifyConsumerRegistration<ConsumerWithCustomDefinition, CustomDefinition>();
    }

    [Fact]
    public void Registers_explicitly_enumerated_consumers()
    {
        configurator.AddConsumersWithDefaultConfiguration(
            new[] { typeof(Consumer1), typeof(ConsumerWithCustomDefinition), typeof(CustomDefinition) },
            typeof(DefaultConsumerDefinition<>)
        );

        VerifyConsumerRegistration<Consumer1, DefaultConsumerDefinition<Consumer1>>();
        VerifyConsumerRegistration<ConsumerWithCustomDefinition, CustomDefinition>();
    }

    [Fact]
    public void Explicit_enumeration_requires_both_consumers_and_definitions()
    {
        configurator.AddConsumersWithDefaultConfiguration(
            new[] { typeof(ConsumerWithCustomDefinition) },
            typeof(DefaultConsumerDefinition<>)
        );

        VerifyConsumerRegistration<
            ConsumerWithCustomDefinition,
            DefaultConsumerDefinition<ConsumerWithCustomDefinition>
        >();
    }

    private void VerifyConsumerRegistration<TConsumer, TDefinition>()
    {
        configurator.Received().AddConsumer(typeof(TConsumer), typeof(TDefinition));
    }
}

public class Message { }

public class Consumer1 : IConsumer<Message>
{
    public Task Consume(ConsumeContext<Message> context) => throw new NotImplementedException();
}

public class Consumer2 : IConsumer<Message>
{
    public Task Consume(ConsumeContext<Message> context) => throw new NotImplementedException();
}

public class ConsumerWithCustomDefinition : IConsumer<Message>
{
    public Task Consume(ConsumeContext<Message> context) => throw new NotImplementedException();
}

public class DefaultConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
    where TConsumer : class, IConsumer { }

public class CustomDefinition : ConsumerDefinition<ConsumerWithCustomDefinition> { }
