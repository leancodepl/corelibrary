using System.Data.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Middleware;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests;

public sealed class RedeliverMessagesTests : IAsyncLifetime, IDisposable
{
    private static readonly Guid MessageId = Guid.NewGuid();

    private readonly IContainer container;
    private readonly InMemoryTestHarness harness;
    private readonly DbConnection dbConnection;

    public RedeliverMessagesTests()
    {
        dbConnection = new SqliteConnection("Filename=:memory:");

        var collection = new ServiceCollection();
        collection.AddMassTransitInMemoryTestHarness();

        var factory = new AutofacServiceProviderFactory();
        var builder = factory.CreateBuilder(collection);
        builder.Register(s => TestDbContext.Create(dbConnection)).As<IConsumedMessagesContext>();

        container = builder.Build();
        harness = container.Resolve<InMemoryTestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(1);

        harness.OnConfigureInMemoryBus += configuration =>
        {
            configuration.UseConcurrencyLimit(1);
            configuration.UseDelayedMessageScheduler();
            configuration.StoreAndPublishDomainEvents(container.Resolve<IServiceProvider>());
            configuration.UseConsumedMessagesFiltering(container.Resolve<IServiceProvider>());
        };
    }

    [Fact]
    public async Task Redelivered_messages_are_not_filtered()
    {
        var consumerHarness = harness.Consumer<MyConsumer>();

        await harness.Start();

        var message = new MyMessage { Text = "test" };
        await harness.Bus.Publish(message, ctx => ctx.MessageId = MessageId);

        Assert.True(await consumerHarness.Consumed.SelectAsync<MyMessage>().Any());

        var messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
        var context = messages.LastOrDefault()?.Context;

        var redeliveryCount = context.GetRedeliveryCount();
        var payload = context.GetPayload<ConsumeContext>();

        Assert.NotNull(payload);
        Assert.Equal(0, redeliveryCount);

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
        context = messages.LastOrDefault()?.Context;
        redeliveryCount = context.GetRedeliveryCount();

        Assert.Equal(2, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());
        Assert.Equal(1, redeliveryCount);

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
        context = messages.LastOrDefault()?.Context;
        redeliveryCount = context.GetRedeliveryCount();

        Assert.Equal(3, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());
        Assert.Equal(2, redeliveryCount);

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
        context = messages.LastOrDefault()?.Context;
        redeliveryCount = context.GetRedeliveryCount();

        Assert.Equal(4, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());
        Assert.Equal(3, redeliveryCount);
    }

    [Fact]
    public async Task Redelivered_message_is_not_filtered()
    {
        var consumerHarness = harness.Consumer<MyConsumer>();

        await harness.Start();

        var message = new MyMessage { Text = "test" };
        await harness.Bus.Publish(message, ctx => ctx.MessageId = MessageId);

        Assert.True(await consumerHarness.Consumed.SelectAsync<MyMessage>().Any());

        var messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();
        var context = messages.LastOrDefault()?.Context;

        var redeliveryCount = context.GetRedeliveryCount();
        var payload = context.GetPayload<ConsumeContext>();

        Assert.NotNull(payload);

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        Assert.Equal(2, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        messages = await consumerHarness.Consumed.SelectAsync<MyMessage>().ToListAsync();

        Assert.Equal(3, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());

        await context.Redeliver(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));

        Assert.Equal(4, await consumerHarness.Consumed.SelectAsync<MyMessage>().Count());
    }

    private sealed class MyConsumer : IConsumer<MyMessage>
    {
        public async Task Consume(ConsumeContext<MyMessage> context)
        {
            await Task.CompletedTask;
        }
    }

    private sealed class MyMessage
    {
        public string Text { get; set; }
    }

    public async Task InitializeAsync()
    {
        await dbConnection.OpenAsync();
        using var dbContext = TestDbContext.Create(dbConnection);
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await harness.Stop();
        await dbConnection.CloseAsync();
        await container.DisposeAsync();
    }

    public void Dispose()
    {
        harness.Dispose();
        dbConnection.Dispose();
        container.Dispose();
    }
}
